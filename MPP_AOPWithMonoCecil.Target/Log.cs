using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_AOPWithMonoCecil.Target
{
    public class Log : Attribute
    {
        private static string fileName = "log.txt";

        public void OnLoggerTargetMethodExecute(System.Reflection.MethodBase method, Dictionary<string, object> parameters, object result = null)
        {
            using(StreamWriter writer = new StreamWriter(new FileStream(fileName, FileMode.Append, FileAccess.Write)))
            {
                writer.Write("CLASS : {" + method.DeclaringType + "}. ");
                writer.Write("METHOD: {" + method.Name + "}. ");
                writer.Write("PARAMETERS: {");
                /*
                if(parameters.Count > 0)
                {
                    foreach (KeyValuePair<string, object> keyValue in parameters)
                    {
                        writer.Write(keyValue.Key + " = " + keyValue.Value);
                    }
                }
                else
                {
                    writer.Write("none");
                }
                writer.Write("} ");
                if (result != null)
                {
                    writer.Write("and RETURNS {" + result + "}");
                }
                */
                writer.WriteLine();
            } 
        }
    }
}
