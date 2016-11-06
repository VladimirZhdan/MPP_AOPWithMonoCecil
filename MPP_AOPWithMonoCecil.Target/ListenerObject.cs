using System;

namespace MPP_AOPWithMonoCecil.Target
{    
    public class ListenerObject
    {        
        [Weak]
        public Delegate Handler0 { get; set; }

        [Weak]
        public Delegate Handler1 { get; set; }

        [Weak]
        public Delegate Handler2 { get; set; }

        [Weak]
        public Delegate Handler3 { get; set; }

        [Weak]
        public Delegate Handler4 { get; set; }

        public ListenerObject()
        {            
            Handler0 = (Action)Handler;
            Handler1 = (Action<int>)Handler;
            Handler2 = (Action<int, double>)Handler;
            Handler3 = (Action<int, double, int>)Handler;
            Handler4 = (Action<int, int, int, int>)Handler;               
        }

        public void Handler01()
        {

        }
         
        public void Handler(int x)
        {
            Console.WriteLine("Handler(int)");
        }

        public void Handler(int x, double y)
        {
            Console.WriteLine("Handler(int, double)");
        }

        public void Handler(int x, double y, int z)
        {
            Console.WriteLine("Handler(int, double, int)");
        }

        public void Handler(int x0, int x1, int x2, int x3)
        {
            Console.WriteLine("Handler(int, int, int, int)");
        }

        public void Handler()
        {
            Console.WriteLine("Handler(void)");
        }
    }
}
