using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using MPP_AOPWithMonoCecil.Target;
using System.Collections.Generic;
using System.IO;

namespace MPP_AOPWithMonoCecil.Tests
{
    [TestClass]
    public class InjectorTest
    {
        private string logFileName;
        object targetAOPObject;
        Dictionary<string, object[]> targetAOPMethodsParameters = new Dictionary<string, object[]>();
        Dictionary<string, MethodInfo> targetAOPMethodsHandlers = new Dictionary<string, MethodInfo>();        

        [TestInitialize]
        public void Initialization()
        {
            logFileName = @"log.txt";            

            string targetFileName = @"e:\Ждан Вова\БГУИР\3 курс\5 семестр\СПП\Лабораторные работы\4-я лаб раб\AOP with Mono.Cecil\MPP_AOPWithMonoCecil\MPP_AOPWithMonoCecil.Target\bin\Debug\MPP_AOPWithMonoCecil.Target.exe"; ;
            Assembly targetAssembly = Assembly.LoadFile(targetFileName);

            targetAOPObject = targetAssembly.CreateInstance(typeof(TargetAopClass).FullName, false, BindingFlags.CreateInstance, null, new object[] { 25 }, null, null);            

            targetAOPMethodsParameters.Add("First", null);
            targetAOPMethodsParameters.Add("Second", new object[] { 1, 2 });

            MethodInfo methodFirst = targetAOPObject.GetType().GetMethod("First");
            MethodInfo methodSecond = targetAOPObject.GetType().GetMethod("Second");

            targetAOPMethodsHandlers.Add("First", methodFirst);
            targetAOPMethodsHandlers.Add("Second", methodSecond);
        }


        [TestMethod]
        public void CheckOutputLogging()
        {
            int initialCountString = File.ReadAllLines(logFileName).Length;                

            int scorerCalls = 0;
            for(int i = 0; i < 10; i++)
            {                
                foreach(KeyValuePair<string, MethodInfo> methodHandler in targetAOPMethodsHandlers)
                {
                    object result = methodHandler.Value.Invoke(targetAOPObject, targetAOPMethodsParameters[methodHandler.Key]);
                    scorerCalls++;                    
                }                                             
            }

            int expectedCountString = initialCountString + scorerCalls;
            int actualCountString = File.ReadAllLines(logFileName).Length;

            Assert.AreEqual(expectedCountString, actualCountString);
        }

        [TestMethod]
        public void CheckRightOutput()
        {            
            for (int i = 0; i < 10; i++)
            {
                foreach (KeyValuePair<string, MethodInfo> methodHandler in targetAOPMethodsHandlers)
                {
                    object result = methodHandler.Value.Invoke(targetAOPObject, targetAOPMethodsParameters[methodHandler.Key]);

                    string expectedLoggingString = GetLoggingString(methodHandler.Value, targetAOPMethodsParameters[methodHandler.Key], result);
                    string actualLoggingString = GetLastStringOfFile(logFileName);

                    Assert.AreEqual(expectedLoggingString, actualLoggingString);                    
                }
            }
        }

        private string GetLastStringOfFile(string fileName)
        {
            string[] allLinesOfFile = File.ReadAllLines(fileName);
            int countStringInFile = allLinesOfFile.Length;
            string result = allLinesOfFile[countStringInFile - 1];
            return result;
        }
        
        private string GetLoggingString(MethodInfo method, object[] parameterValues, object resultOfMethod)
        {
            string result = ("CLASS: {" + method.DeclaringType.Name + "}. ");
            result += ("METHOD: {" + method.Name + "}. ");
            result += ("PARAMETERS: {");

            //add parameters
            Dictionary<string, object> parameters = GetParametersFromMethod(method, parameterValues);

            if(parameters.Count > 0)
            {
                int counter = 0;
                foreach (KeyValuePair<string, object> keyValue in parameters)
                {
                    result += keyValue.Key + " = " + keyValue.Value;
                    if (counter != (parameters.Count - 1))
                    {
                        result += ", ";
                    }                    
                    counter++;
                }                
            }
            else
            {
                result += "none";
            }            
            result += "} ";

            if(method.ReturnType.Name != "Void")
            {
                result += ("and RETURNS {" + resultOfMethod + "}");
            }
            
            return result;
        }

        private Dictionary<string, object> GetParametersFromMethod(MethodInfo method, object[] parameterValues)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            ParameterInfo[] parameterInfos = method.GetParameters();

            for(int i = 0; i < parameterInfos.Length; i++)
            {
                result.Add(parameterInfos[i].Name, parameterValues[i]);
            }            

            return result;
        }
    }
}
