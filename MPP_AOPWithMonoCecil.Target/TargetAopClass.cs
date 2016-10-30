using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_AOPWithMonoCecil.Target
{
    [Log]
    public class TargetAopClass
    {
        private int parameter;

        public TargetAopClass(int parameter)
        {
            this.parameter = parameter;
        }

        public void First()
        {
            Console.WriteLine("First Function");

            
            //Log log = new Log();

            //System.Reflection.MethodBase baseMethod = System.Reflection.MethodBase.GetCurrentMethod();
            //System.Reflection.ParameterInfo[] parameters = baseMethod.GetParameters();
            //Dictionary<string, object>[] dictionary = new Dictionary<string, object>[parameters.Length];
            //for(int i = 0; i < dictionary.Length; i++)
            //{                
            //    dictionary[i].Add(parameters[i].Name, parameters[i].)
            //}


            //log.OnLoggerTargetMethodExecute(System.Reflection.MethodBase.GetCurrentMethod(), )
        }

        public int Second(int parameter1, object parameter2)
        {

            return 10;
        }
    }
}
