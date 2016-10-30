using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;


namespace MPP_AOPWithMonoCecil
{
    public class Injector
    {
        private string assemblyPath;

        public Injector(string path)
        {
            assemblyPath = path;
        }

        public void Inject()
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            TypeDefinition logTypeDefenition = GetTypeDefFromAssembly(assembly, "Log");

            TypeReference methodBaseType = assembly.MainModule.Import(typeof(System.Reflection.MethodBase));
            IEnumerable<TypeDefinition> enumerablesTypesWithLogAttribute = GetTypesWithAttribute(assembly, logTypeDefenition);
            IEnumerator<TypeDefinition> typesEnumerator = enumerablesTypesWithLogAttribute.GetEnumerator();
            int countTypesToInject = enumerablesTypesWithLogAttribute.Count();
            for (int i = 0; i < countTypesToInject; i++)
            {
                typesEnumerator.MoveNext();
                TypeDefinition typeDef = typesEnumerator.Current;
                InjectType(typeDef, assembly, logTypeDefenition, methodBaseType);
            }                    
            assembly.Write(assemblyPath);          
        }

        private void InjectType(TypeDefinition type, AssemblyDefinition assembly, TypeDefinition logTypeDefenition, TypeReference methodBaseType)
        {
            MethodDefinition[] typeMethods = type.Methods.ToArray();
            foreach (var method in typeMethods)
            {
                InjectMethod(method, assembly, logTypeDefenition, methodBaseType);
            }
        }

        private void InjectMethod(MethodDefinition method, AssemblyDefinition assembly, TypeDefinition logTypeDefenition, TypeReference methodBaseType)
        {
            MethodInfo writeLineMethod = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });
            MethodReference writeLineRef = assembly.MainModule.Import(writeLineMethod);
            var methodILProcessor = method.Body.GetILProcessor();


            var attributeRef = assembly.MainModule.Import(typeof(CustomAttribute));
            var typeRef = assembly.MainModule.Import(typeof(Type));


            //declayring local variables            
            var currentMethodBaseVar = new VariableDefinition(methodBaseType);
            var currentDeclayringTypeVar = new VariableDefinition(typeRef);

            var logAttributeVar = new VariableDefinition(attributeRef);


            //create local variables
            methodILProcessor.Body.Variables.Add(currentMethodBaseVar);
            methodILProcessor.Body.Variables.Add(logAttributeVar);
            methodILProcessor.Body.Variables.Add(currentDeclayringTypeVar);


            //create methods reference
            MethodReference getCurrentMethodBaseRef = assembly.MainModule.Import(typeof(System.Reflection.MethodBase).GetMethod("GetCurrentMethod"));
            MethodReference getDeclayringTypeRef = assembly.MainModule.Import(typeof(System.Reflection.MemberInfo).GetMethod("get_DeclaringType"));
            MethodReference getTypeFromHandleRef = assembly.MainModule.Import(typeof(Type).GetMethod("GetTypeFromHandle",
                new Type[] { typeof(RuntimeTypeHandle) }));
            MethodReference getCustomAttributeRef = assembly.MainModule.Import(typeof(Attribute).GetMethod("GetCustomAttribute",
                new Type[] { typeof(System.Reflection.MemberInfo), typeof(Type) }));


            List<Instruction> insertInstuctionList = new List<Instruction>();

            //set variables
            insertInstuctionList.Add(Instruction.Create(OpCodes.Call, getCurrentMethodBaseRef));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Stloc, currentMethodBaseVar));


            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldloc, currentMethodBaseVar));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Callvirt, getDeclayringTypeRef));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Stloc, currentDeclayringTypeVar));
            //
            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldloc, currentDeclayringTypeVar));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldtoken, logTypeDefenition));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Call, getTypeFromHandleRef));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Call, getCustomAttributeRef));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Castclass, logTypeDefenition));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Stloc, logAttributeVar));



            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldstr, "Inject"));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Call, writeLineRef));

            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldstr, method.DeclaringType.ToString()));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Call, writeLineRef));


            MethodDefinition onLoggerTargetMethodExecute = GetOnLoggerTargetMethodExecuteDef(logTypeDefenition);


            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldloc, logAttributeVar));
            //this paramether = log

            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldloc, currentMethodBaseVar));
            //method = currentMethodBaseVar

            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldnull));
            //parameters = null

            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldnull));
            //result = null

            insertInstuctionList.Add(Instruction.Create(OpCodes.Callvirt, onLoggerTargetMethodExecute));
            //OnLoggerTargetMethodExecute(method, parameters, result);

            InsertInjectInstructionsIntoMethod(methodILProcessor, insertInstuctionList.ToArray());
        }

        private void InsertInjectInstructionsIntoMethod(ILProcessor methodILProcessor, Instruction[] instruction)
        {
            for(int i = 0; i < instruction.Length; i++)
            {
                methodILProcessor.Body.Instructions.Insert(methodILProcessor.Body.Instructions.Count - 1, instruction[i]);
            }                        
        }

        private TypeDefinition GetTypeDefFromAssembly(AssemblyDefinition assembly, String typeName)
        {            
            Mono.Collections.Generic.Collection<TypeDefinition> assemblyTypes = assembly.MainModule.Types;
            foreach (TypeDefinition typeDefinitionElem in assemblyTypes)
            {
                if (typeDefinitionElem.Name == typeName)
                {
                    return typeDefinitionElem;                    
                }
            }            
            return null;
        }

        private IEnumerable<TypeDefinition> GetTypesWithAttribute(
            AssemblyDefinition assembly, TypeDefinition logAttributeTypeDef)
        {
            return assembly.MainModule.Types.ToArray().Where(
                typeDef => (typeDef.CustomAttributes.Where(
                                attribute => attribute.AttributeType == logAttributeTypeDef).Count() != 0)
            );
        }

        private MethodDefinition GetOnLoggerTargetMethodExecuteDef(TypeDefinition logTypeDefinition)
        {
            Mono.Collections.Generic.Collection<MethodDefinition> methodsDefinitions = logTypeDefinition.Methods;
            foreach (MethodDefinition methodDefinition in methodsDefinitions)
            {
                if (methodDefinition.Name == "OnLoggerTargetMethodExecute")
                {
                    return methodDefinition;
                }
            }
            return null;
        }

    }
}
