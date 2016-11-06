using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MPP_AOPWithMonoCecil
{
    public class WeakInjector
    {
        private string assemblyPath;

        public WeakInjector(string path)
        {
            assemblyPath = path;
        }

        public void Inject()
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            TypeDefinition weakTypeDefinition = GetTypeDefFromAssembly(assembly, "Weak");
            IEnumerable<TypeDefinition> enumerableTypesWithPropertyWithWeakAttribute = GetTypesWithPropertyWithAttribute(assembly, weakTypeDefinition);            
            IEnumerator<TypeDefinition> typesEnumerator = enumerableTypesWithPropertyWithWeakAttribute.GetEnumerator();
            int countTypesToInject = enumerableTypesWithPropertyWithWeakAttribute.Count();
            for (int i = 0; i < countTypesToInject; i++)
            {
                typesEnumerator.MoveNext();
                TypeDefinition typeDef = typesEnumerator.Current;
                InjectType(typeDef, assembly, weakTypeDefinition);
            }
            assembly.Write(assemblyPath);
            typesEnumerator.Dispose();
        }

        private void InjectType(TypeDefinition type, AssemblyDefinition assembly, TypeDefinition weakTypeDefinition)
        {
            IEnumerable<PropertyDefinition> enumerablePropertiesWithAttribute = type.Properties.Where(
                    property => property.CustomAttributes.Where(
                        attribute => attribute.AttributeType == weakTypeDefinition).Count() != 0
                        );
            IEnumerator<PropertyDefinition> propertiesEnumerator = enumerablePropertiesWithAttribute.GetEnumerator();
            int countPropertiesToInject = enumerablePropertiesWithAttribute.Count();            
            for(int i = 0; i < countPropertiesToInject; i++)
            {
                propertiesEnumerator.MoveNext();
                PropertyDefinition propertyDef = propertiesEnumerator.Current;
                InjectProperty(propertyDef, assembly, weakTypeDefinition);
            }                

            propertiesEnumerator.Dispose();
        }

        private void InjectProperty(PropertyDefinition property, AssemblyDefinition assembly, TypeDefinition weakTypeDefinition)
        {
            MethodDefinition setMethod = property.SetMethod;            
            InjectSetMethodOfProperty(setMethod, assembly, weakTypeDefinition, property.Name);            
        }

        private void InjectSetMethodOfProperty(MethodDefinition method, AssemblyDefinition assembly, TypeDefinition weakTypeDefinition, string propertyName)
        {
            List<Instruction> insertInstuctionList = new List<Instruction>();

            var methodILProcessor = method.Body.GetILProcessor();

            TypeDefinition listenerObjectTypeDefinition = GetTypeDefFromAssembly(assembly, "ListenerObject");            

            //create types reference
            var propertyInfoType = assembly.MainModule.Import(typeof(System.Reflection.PropertyInfo));
            var customAttributeType = assembly.MainModule.Import(typeof(CustomAttribute));
            var delegateType = assembly.MainModule.Import(typeof(Delegate));

            //declayring local variables            
            var propertyVar = new VariableDefinition(propertyInfoType);
            var weakAttributeVar = new VariableDefinition(customAttributeType);
            var handlerVar = new VariableDefinition(delegateType);

            //create local variables
            methodILProcessor.Body.Variables.Add(propertyVar);
            methodILProcessor.Body.Variables.Add(weakAttributeVar);
            methodILProcessor.Body.Variables.Add(handlerVar);


            //create methods references
            MethodReference getTypeFromHandleRef = assembly.MainModule.Import(typeof(Type).GetMethod("GetTypeFromHandle",
                new Type[] { typeof(RuntimeTypeHandle) }));
            MethodReference getPropertyRef = assembly.MainModule.Import(typeof(Type).GetMethod("GetProperty",
                new Type[] { typeof(string) }));
            MethodReference getCustomAttributeRef = assembly.MainModule.Import(typeof(Attribute).GetMethod("GetCustomAttribute",
                new Type[] { typeof(System.Reflection.MemberInfo), typeof(Type) }));

            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldtoken, listenerObjectTypeDefinition));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Call, getTypeFromHandleRef));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldstr, propertyName));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Call, getPropertyRef));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Stloc, propertyVar));
            //PropertyInfo property = typeof(ListenerObject).GetProperty(propertyName);

            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldloc, propertyVar));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldtoken, weakTypeDefinition));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Call, getTypeFromHandleRef));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Call, getCustomAttributeRef));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Isinst, weakTypeDefinition));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Stloc, weakAttributeVar));
            //Weak weakAttribute = Attribute.GetCustomAttribute(property, typeof(Weak)) as Weak;

            MethodDefinition getWeakDelegateMethod = GetMethodFromType(weakTypeDefinition, "GetWeakDelegate");

            foreach (var arg in method.Parameters)
            {
                if (arg.ParameterType.Name == delegateType.Name)
                {
                    insertInstuctionList.Add(Instruction.Create(OpCodes.Ldarg, arg));
                    insertInstuctionList.Add(Instruction.Create(OpCodes.Stloc, handlerVar));
                }
            }

            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldloc, weakAttributeVar));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Ldloc, handlerVar));
            insertInstuctionList.Add(Instruction.Create(OpCodes.Callvirt, getWeakDelegateMethod));
            //call weakAttributeVar.GetWeakDelegate(handlerVar);

            InsertInjectInstructionsIntoMethod(methodILProcessor, insertInstuctionList.ToArray());
        }

        private void InsertInjectInstructionsIntoMethod(ILProcessor methodILProcessor, Instruction[] instruction)
        {
            methodILProcessor.Body.Instructions.RemoveAt(methodILProcessor.Body.Instructions.Count - 3);
            for (int i = 0; i < instruction.Length; i++)
            {
                methodILProcessor.Body.Instructions.Insert(methodILProcessor.Body.Instructions.Count - 2, instruction[i]);
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

        private IEnumerable<TypeDefinition> GetTypesWithPropertyWithAttribute(
            AssemblyDefinition assembly, TypeDefinition weakAttributeTypeDef)
        {
            return assembly.MainModule.Types.ToArray().Where(
                typeDef => (typeDef.Properties.Where(
                                property => property.CustomAttributes.Where(
                                    attribute => attribute.AttributeType == weakAttributeTypeDef).Count() != 0).Count() != 0)                                      
            );
        }

        private MethodDefinition GetMethodFromType(TypeDefinition logTypeDefinition, string methodName)
        {
            Mono.Collections.Generic.Collection<MethodDefinition> methodsDefinitions = logTypeDefinition.Methods;
            foreach (MethodDefinition methodDefinition in methodsDefinitions)
            {
                if (methodDefinition.Name == methodName)
                {
                    return methodDefinition;
                }
            }
            return null;
        }
    }
}
