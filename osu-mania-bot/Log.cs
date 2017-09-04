using System;
using System.IO;
using System.Text;

namespace Amatsu
{
    class Log
    {
        private static string _date {get; set;}

        public static void Init()
        {
            Console.WriteLine("Initialization started...");
            _date = DateTime.Now.ToShortDateString().Replace('.', '-').Replace('/', '-');
        }

        public static void Write(string thingToWrite)
        {
            var logText = new StringBuilder();
            logText.Append($"{DateTime.Now}: {thingToWrite}\r\n");
            File.AppendAllText(_date + "_logfile.log", logText.ToString());
            logText.Clear();
        }

        public static void Report(string thingToWrite)
        {
            var logText = new StringBuilder();
            logText.Append($"{DateTime.Now}: {thingToWrite}\r\n");
            File.AppendAllText(_date + "_reportfile.log", logText.ToString());
            logText.Clear();
        }
    }
}
