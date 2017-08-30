using System;   
using System.Threading;
using Meebey.SmartIrc4net;
using System.Text;
using System.Collections.Generic;

namespace Amatsu
{
    class Program
    {
        public static IrcClient irc = new IrcClient();
        public static List<string> users = new List<string>();
        public static string[] last_map = new string[2048];
        public static bool _r = true;
        public static bool _np = true;
        public static bool _acc = true;

        public static void OnQueryAction(object sender, ActionEventArgs e)
        {
            Log.Write(e.Data.RawMessage);
            if (_np)
            {
                if (e.Data.RawMessage.Contains("https://osu.ppy.sh/b/"))
                {
                    try
                    {
                        bool checkpoint = false;
                        Console.WriteLine(e.Data.Nick + ": " + e.Data.Message);
                        string map_id = e.Data.Message.Substring(e.Data.Message.IndexOf("sh/b/") + 5);
                        try
                        {
                            map_id = map_id.Remove(map_id.IndexOf(" "));
                            if (map_id.Contains("?"))
                                map_id = map_id.Remove(map_id.IndexOf("?"));
                        }
                        catch { }

                        
                        for (int i = 0; i < users.Count; i++)
                        {
                            if (users[i] == (e.Data.Nick))
                            {
                                last_map[i] = map_id;
                                checkpoint = true;
                            }
                        }

                        if (checkpoint == false)
                        {
                            users.Add(e.Data.Nick);
                            users.Add(map_id);
                        }

                        
                        MapInfo m = new MapInfo(map_id);
                        if (m.mode == "3")
                            irc.SendMessage(SendType.Message, e.Data.Nick, $"Map info: 92%: {Data.Calculate(m.od, m.stars, m.obj, 92)}pp | 95%: {Data.Calculate(m.od, m.stars, m.obj, 95)}pp | 98%: {Data.Calculate(m.od, m.stars, m.obj, 98)}pp");
                        else irc.SendMessage(SendType.Message, e.Data.Nick, "Mania mode requiered.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex);
                        Log.Write("Error: " + ex);
                        irc.SendMessage(SendType.Message, e.Data.Nick, "Error occuried.");
                    }
                }
            }
        }

        public static void OnQueryMessage(object sender, IrcEventArgs e)
        {
            try {
                Log.Write("Query: " + e.Data.RawMessage);
                Console.WriteLine(e.Data.Nick + ": " + e.Data.Message);
                if (e.Data.Message.StartsWith("!r"))
                {
                    if (_r)
                    {
                        if (e.Data.Message == "!r")
                            irc.SendReply(e.Data, "This command sends you a map recommendation. Works with 4 and 7 keys atm. Example: !r 4");
                        if (e.Data.Message.Contains("!r 4"))
                        {
                            Double pp = Osu.GetAveragePP(e.Data.Nick);
                            irc.SendReply(e.Data, Data.GetMap(pp, "4"));
                            Console.WriteLine(".");
                        }
                        else if (e.Data.Message.Contains("!r 7"))
                        {
                            Double pp = Osu.GetAveragePP(e.Data.Nick);
                            irc.SendReply(e.Data, Data.GetMap(pp, "7"));
                            Console.WriteLine(".");
                        }
                        else
                        {
                            irc.SendReply(e.Data, "Currently works with 4 and 7 only.");
                        }
                    }
                }
                if (e.Data.Message.StartsWith("!helpmeplz"))
                {
                    Log.Write("===================REPORT====================");
                    Log.Write(e.Data.RawMessage);
                    Log.Write("===================REPORT====================");
                    Log.Report(e.Data.RawMessage);
                    irc.SendReply(e.Data, "Report sent. Thanks for help!");
                }
                if (e.Data.Message.StartsWith("!acc"))
                {
                    string[] args = e.Data.Message.Substring(e.Data.Message.IndexOf(" ")+1).Split(' ');
                    if (args.Length!=2)
                    {
                        irc.SendReply(e.Data, "Too few arguments. Example: !acc 96,12 1234567");
                        return;
                    }
                    if (_acc)
                    {
                        string map_id = null;
                        for (int i = 0; i < users.Count; i++)
                        {
                            if (users[i] == e.Data.Nick)
                            {
                                try
                                {
                                    map_id = last_map[i];
                                }
                                catch(ArgumentOutOfRangeException ex)
                                {
                                    Log.Write(""+ex);
                                    Console.WriteLine("Error (ArgumentOutOfRangeException)");
                                    irc.SendReply(e.Data, "Send /np first.");
                                }
                            }
                        }

                        Double acc;
                        Double score;
                        try
                        {
                            acc = Convert.ToDouble(args[0].Replace('.', ','));
                            score = Convert.ToDouble(args[1]);
                            if (acc < 0 || acc > 100) acc = -1;
                            if (score < 0 || score > 1000000) acc = -1;
                        }
                        catch (Exception ex)
                        {
                            Log.Write("Error: " + ex);
                            Console.WriteLine(ex);
                            acc = -1;
                            score = -1;
                        }
                        if (acc == -1 || score == -1)
                        {
                            irc.SendReply(e.Data, "Wrong input. Usage: !acc [acc] [score]");
                        }
                        else
                        {
                            MapInfo m = new MapInfo(map_id);
                            if (m.mode == "3")
                            {
                                string output = $"About {Data.Calculate(m.od, m.stars, m.obj, acc, score)}pp for {acc}%";
                                irc.SendReply(e.Data, output);
                            }
                            else
                            {
                                irc.SendReply(e.Data, "Mania mode required.");
                                Log.Write("Error: Wrong mode.");
                            }
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                Log.Write("Error: " + ex);
                Console.WriteLine(ex);
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
            string pass = "pass";
            users.Add("-_Alexmal_-");
            last_map[0] = "425725";
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
                if (cmd.StartsWith("/r"))
                {
                    if (_r)
                    {
                        _r = false;
                        Console.WriteLine("Changed _r to " + _r);
                    }
                    else
                    {
                        _r = true;
                        Console.WriteLine("Changed _r to " + _r);
                    }
                }

                if (cmd.StartsWith("/acc"))
                {
                    if (_acc)
                    {
                        _acc = false;
                        Console.WriteLine("Changed _acc to " + _acc);
                    }
                    else
                    {
                        _acc = true;
                        Console.WriteLine("Changed _r to " + _acc);
                    }
                }

                if (cmd.StartsWith("/np"))
                {
                    if (_np)
                    {
                        _np = false;
                        Console.WriteLine("Changed _np to " + _np);
                    }
                    else
                    {
                        _np = true;
                        Console.WriteLine("Changed _np to " + _np);
                    }
                }
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


