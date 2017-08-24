using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amatsu
{
    class Log
    {
        private static string _date {get;set;}

        public static void Init()
        {
            Console.WriteLine("Initialization started...");
            _date = DateTime.Now.ToShortDateString().Replace('.', '-');
        }

        public static void Write(string ThingToWrite)
        {
            using (StreamWriter sw = File.AppendText(_date + "_logfile"))
            {
                sw.WriteLine(DateTime.Now + ":  " + ThingToWrite);
            }
        }
    }
}
