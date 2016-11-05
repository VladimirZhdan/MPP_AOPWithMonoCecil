using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
