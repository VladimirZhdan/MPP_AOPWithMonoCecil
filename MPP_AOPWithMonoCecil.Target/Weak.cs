using System;

namespace MPP_AOPWithMonoCecil.Target
{
    public class Weak : Attribute
    {        
        public Weak()
        {            
        }

        public Delegate GetWeakDelegate(Delegate handler)
        {            
            return new WeakDelegate(handler).Weak;
        }        
    }
}
