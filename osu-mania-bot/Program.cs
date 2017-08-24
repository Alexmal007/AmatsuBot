using System;   
using System.Threading;
using Meebey.SmartIrc4net;
using System.Text;

namespace Amatsu
{
    class Program
    {
        public static IrcClient irc = new IrcClient();

        public static void OnQueryAction(object sender, ActionEventArgs e)
        {
            if (e.Data.RawMessage.Contains("https://osu.ppy.sh/b/"))
            {
                try
                {
                    Console.WriteLine(e.Data.Nick + ": " + e.Data.Message);
                    string map_id = e.Data.Message.Substring(e.Data.Message.IndexOf("sh/b/")+5);
                    try
                    {
                        map_id = map_id.Remove(map_id.IndexOf(" "));
                        if(map_id.Contains("?"))
                            map_id = map_id.Remove(map_id.IndexOf("?"));
                    }
                    catch { }                 
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                    Log.Write("Error: " + ex);
                    irc.SendMessage(SendType.Message, e.Data.Nick, "Error occuried.");
                }
            }

        }


        public static void OnQueryMessage(object sender, IrcEventArgs e)
        {
            Log.Write("Query: " + e.Data.RawMessage);
            Console.WriteLine(e.Data.Nick + ": " + e.Data.Message);
            if (e.Data.Message.StartsWith("!r"))
            {
                if (e.Data.Message == "!r")
                    irc.SendReply(e.Data,"This command sends you a map recommendation. Works with 4 and 7 keys atm. Example: !r 4");
                if(e.Data.Message.Contains("!r 4"))
                {
                    Double pp = Osu.GetAveragePP(e.Data.Nick);
                    irc.SendReply(e.Data,Data.GetMap(pp,"4"));
                }
                if(e.Data.Message.Contains("!r 7"))
                {
                    Double pp = Osu.GetAveragePP(e.Data.Nick);
                    irc.SendReply(e.Data, Data.GetMap(pp, "7"));
                }
            }
            
        }

        public static void OnError(object sender, ErrorEventArgs e)
        {
            Log.Write("Error: " + e.ErrorMessage);
            Console.WriteLine("Error: " + e.ErrorMessage);
            Exit();
        }

        public static void OnRawMessage(object sender, IrcEventArgs e)
        {
            if(!e.Data.RawMessage.Contains("QUIT") && !e.Data.RawMessage.Contains("JOIN") && e.Data.RawMessage != null && !e.Data.RawMessage.Contains("PRIVMSG") && !e.Data.RawMessage.Contains("PING") && !e.Data.RawMessage.Contains("PONG"))
            {
                Log.Write(e.Data.RawMessage);
                Console.WriteLine(e.Data.RawMessage);
            }
            
        }

        static void Main(string[] args)
        {
            irc.Encoding = Encoding.UTF8;
            Log.Init();
            Console.Title = "Amatsu!";
            irc.SendDelay = 200;
            irc.ActiveChannelSyncing = true;
            irc.OnQueryMessage += new IrcEventHandler(OnQueryMessage);
            irc.OnError += new ErrorEventHandler(OnError);
            irc.OnRawMessage += new IrcEventHandler(OnRawMessage);
            irc.OnQueryAction += new ActionEventHandler(OnQueryAction);
            string server = "irc.ppy.sh";
            int port = 6667;
            string username = "-_Alexmal_-";
            string pass = "85d3d179";
            try
            {
                irc.Connect(server, port);
                irc.Login(username, username, 0, "", pass);
                new Thread(new ThreadStart(ReadCommands)).Start();
                irc.Listen();
            }
            catch (ConnectionException e)
            {
                Console.WriteLine("Couldn't connect! Reason: " + e.Message);
                Console.ReadLine();
            }
        }
        public static void ReadCommands()
        {
            while (true)
            {
                string cmd = Console.ReadLine();
                
                if (cmd.StartsWith("/test"))
                {
                    irc.SendMessage(SendType.Message, "-_Alexmal_-", "Test command initiated.");
                }

                if (cmd.StartsWith("/clear"))
                    Console.Clear();
            }
        }

        public static void Exit()
        {
            Console.WriteLine("Exiting...");
            Log.Write("Exit.");
            Environment.Exit(0);
        }

    }

}


