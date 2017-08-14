using System;   
using System.Threading;
using Meebey.SmartIrc4net;

namespace osu_mania_bot
{
    class Program
    {
        public static IrcClient irc = new IrcClient();

        public static void OnQueryMessage(object sender, IrcEventArgs e)
        {
            Log.Write("Recieved Query: " + e.Data.RawMessage);
            Console.WriteLine("PM from " + e.Data.Nick + ": " + e.Data.Message);
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
            if(!e.Data.RawMessage.Contains("QUIT") && !e.Data.RawMessage.Contains("JOIN") && e.Data.RawMessage != null && !e.Data.RawMessage.Contains("PRIVMSG"))
            {
                Log.Write("Recieved: " + e.Data.RawMessage);
                Console.WriteLine("Recieved" + e.Data.RawMessage);
            }
            
        }

        static void Main(string[] args)
        {
            Log.Init();
            Console.Title = "osu!bot";
            irc.SendDelay = 200;
            irc.ActiveChannelSyncing = true;
            irc.OnQueryMessage += new IrcEventHandler(OnQueryMessage);
            irc.OnError += new ErrorEventHandler(OnError);
            irc.OnRawMessage += new IrcEventHandler(OnRawMessage);
            string server = "irc.ppy.sh";
            int port = 6667;
            string username = "-A_l_e_x_m_a_L-";
            string pass = "pass";
            try
            {
                irc.Connect(server, port);
                irc.Login(username, username, 0, "", pass);
                new Thread(new ThreadStart(ReadCommands)).Start();
                irc.Listen();
            }
            catch (ConnectionException e)
            {
                Console.WriteLine("couldn't connect! Reason: " + e.Message);
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
                    irc.SendMessage(SendType.Message, "-A_l_e_x_m_a_l-", "Test command initiated.");
                }

                if (cmd.StartsWith("/clear"))
                    Console.Clear();
            }
        }

        public static void Exit()
        {
            Console.WriteLine("Exiting...");
            Environment.Exit(0);
        }

    }

}


