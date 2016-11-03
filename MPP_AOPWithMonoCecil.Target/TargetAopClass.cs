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
        }

        public int Second(int parameter1, object parameter2)
        {            
            return 10;
        }
    }
}
