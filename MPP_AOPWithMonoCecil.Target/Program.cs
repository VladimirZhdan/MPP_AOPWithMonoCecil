using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_AOPWithMonoCecil.Target
{
    class Program
    {                           
        static void Main(string[] args)
        {
            var target = new TargetAopClass(25);
            target.First();
            target.Second(5, 20);
            

            ListenerObject listener = new ListenerObject();
            SourceObject source = new SourceObject();
            Console.Write("Press <Enter> to exit...");
            Console.ReadLine();



        }
    }
}
