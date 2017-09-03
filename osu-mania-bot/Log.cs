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
            var logText = new StringBuilder();
            logText.Append($"{DateTime.Now}: {ThingToWrite}\r\n");
            File.AppendAllText(_date + "_logfile", logText.ToString());
            logText.Clear();
        }
        public static void Report(string ThingToWrite)
        {
            var logText = new StringBuilder();
            logText.Append($"{DateTime.Now}: {ThingToWrite}\r\n");
            File.AppendAllText(_date + "_reportfile", logText.ToString());
            logText.Clear();
        }
    }
}
