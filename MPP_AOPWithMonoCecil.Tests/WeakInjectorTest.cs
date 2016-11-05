using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using MPP_AOPWithMonoCecil.Target;

namespace MPP_AOPWithMonoCecil.Tests
{
    [TestClass]
    public class WeakInjectorTest
    {
        private object[] listenerObject = new object[10];
        private string[] propertiesName = new string[5];
        private MethodInfo[,] getPropertyMethods = new MethodInfo[10,5];

        [TestInitialize]
        public void Initialization()
        {            
            for(int i = 0; i < propertiesName.Length; i++)
            {
                propertiesName[i] = ("Handler" + i);
            }

            string targetFileName = @"e:\Ждан Вова\БГУИР\3 курс\5 семестр\СПП\Лабораторные работы\4-я лаб раб\AOP with Mono.Cecil\MPP_AOPWithMonoCecil\MPP_AOPWithMonoCecil.Target\bin\Debug\MPP_AOPWithMonoCecil.Target.exe"; ;
            Assembly targetAssembly = Assembly.LoadFile(targetFileName);    
            
            for(int i = 0; i < listenerObject.Length; i++)
            {                                
                listenerObject[i] = targetAssembly.CreateInstance(typeof(ListenerObject).FullName);
                for(int j = 0; j < propertiesName.Length; j++)
                {
                    getPropertyMethods[i, j] = listenerObject[i].GetType().GetProperty(propertiesName[j]).GetMethod;                    
                }                                                                                
            }                    
        }

        [TestMethod]
        public void CheckDecreaseMemory()
        {
            SourceObject sourceObject = new SourceObject();            
            for (int i = 0; i < 10; i++)
            {
                sourceObject.Completed0 += (Action)getPropertyMethods[i, 0].Invoke(listenerObject[i], null);
                sourceObject.Completed += (Action<int>)getPropertyMethods[i, 1].Invoke(listenerObject[i], null);
                sourceObject.Completed1 += (Action<int, double>)getPropertyMethods[i, 2].Invoke(listenerObject[i], null);
                sourceObject.Completed2 += (Action<int, double, int>)getPropertyMethods[i, 3].Invoke(listenerObject[i], null);
                sourceObject.Completed3 += (Action<int, int, int, int>)getPropertyMethods[i, 4].Invoke(listenerObject[i], null);                
            }

            long initialMemoryCount = GC.GetTotalMemory(true);
            for (int i = 0; i < 10; i++)
            {
                listenerObject[i] = null;
            }
            GC.Collect(2, GCCollectionMode.Forced);

            long currentMemoryCount = GC.GetTotalMemory(true);

            bool expectedResult = true;

            bool actualResult = (currentMemoryCount < initialMemoryCount);

            Assert.AreEqual(expectedResult, actualResult);
        }        
    }
}
