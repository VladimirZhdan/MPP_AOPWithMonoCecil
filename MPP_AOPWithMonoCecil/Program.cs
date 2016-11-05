using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_AOPWithMonoCecil
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = @"e:\Ждан Вова\БГУИР\3 курс\5 семестр\СПП\Лабораторные работы\4-я лаб раб\AOP with Mono.Cecil\MPP_AOPWithMonoCecil\MPP_AOPWithMonoCecil.Target\bin\Debug\MPP_AOPWithMonoCecil.Target.exe";
            if (args.Length != 0)
            {
                fileName = args[0];
            }            
            //    Injector injector = new Injector(fileName);
            //    injector.Inject();

            WeakInjector weakInjector = new WeakInjector(fileName);
            weakInjector.Inject();        
        }
    }
}
