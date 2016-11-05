using System;
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

            //targetAOPObject = Activator.CreateInstance(targetFileName, typeof(TargetAopClass).FullName, )

            //targetAOPObject = targetAssembly.CreateInstance(typeof(TargetAopClass).FullName);

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
    }
}
