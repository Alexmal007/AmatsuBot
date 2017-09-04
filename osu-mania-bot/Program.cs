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
        public static List<string> last_map = new List<string>();
        public static string password = null;
        public static string username = null;
        public static bool _r = true;
        public static bool _np = true;
        public static bool _acc = true;

        // Here we catch user's /np
        public static void OnQueryAction(object sender, ActionEventArgs e)
        {
            Log.Write(e.Data.RawMessage);
            Console.WriteLine(e.Data.Message);
            //check if this command is enabled
            if (_np && (e.Data.RawMessage.Contains("https://osu.ppy.sh/b/") || e.Data.RawMessage.Contains("http://osu.ppy.sh/b/")))
            {
                try
                {
                    //we need this to check if the user is in the list (we need it for !acc command) 
                    bool checkpoint = false;
                    Console.WriteLine(e.Data.Nick + ": " + e.Data.Message);
                    string map_id = e.Data.Message.Substring(e.Data.Message.IndexOf("sh/b/") + 5);

                    map_id = map_id.Remove(map_id.IndexOf(" "));
                    if (map_id.Contains("?"))
                        map_id = map_id.Remove(map_id.IndexOf("?"));


                    Console.WriteLine("Map ID: " + map_id);

                    //here we check if the user is in ther list and set map for him if needed
                    for (int i = 0; i < users.Count; i++)
                    {
                        if (users[i] == (e.Data.Nick))
                        {
                            last_map[i] = map_id;
                            checkpoint = true;
                        }
                    }

                    //if the user wasn't found in list
                    if (!checkpoint)
                    {
                        users.Add(e.Data.Nick);
                        last_map.Add(map_id);
                    }

                    var m = new MapInfo(map_id);
                    if (m.mode == "3")
                    {
                        var output = $"{m.artist} - {m.title} [{m.version}] | 92%: {Data.Calculate(m.od, m.stars, m.obj, 92)}pp | 95%: {Data.Calculate(m.od, m.stars, m.obj, 95)}pp | 98%: {Data.Calculate(m.od, m.stars, m.obj, 98)}pp";
                        Log.Write(output);
                        irc.SendMessage(SendType.Message, e.Data.Nick, output);
                    }
                    else
                    {
                        irc.SendMessage(SendType.Message, e.Data.Nick, "Mania mode required.");
                        Log.Write("Mania mode required. m.mode = " + m.mode);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                    Log.Write("Error: " + ex);
                    irc.SendMessage(SendType.Message, e.Data.Nick, "Error occuried.");
                }

            }
        }

        //here we recieve all pm commands
        public static void OnQueryMessage(object sender, IrcEventArgs e)
        {
            var message = e.Data.Message.ToLower();

            try
            {
                Log.Write("Query: " + e.Data.RawMessage);
                Console.WriteLine(e.Data.Nick + ": " + message);

                if (message.StartsWith("!r") && _r)
                {
                    if (message == "!r")
                        irc.SendReply(e.Data, "This command sends you a map recommendation. Works with 4 and 7 keys atm. Example: !r 4");
                    if (message.Contains("!r 4"))
                    {
                        Double pp = Osu.GetAveragePP(e.Data.Nick);
                        Log.Write($"{pp}");
                        var get_map = Data.GetMap(pp, "4");
                        Log.Write(get_map);
                        irc.SendReply(e.Data, get_map);
                        Console.WriteLine("~");
                    }
                    else if (message.Contains("!r 7"))
                    {
                        Double pp = Osu.GetAveragePP(e.Data.Nick);
                        Log.Write($"{pp}");
                        var get_map = Data.GetMap(pp, "7");
                        Log.Write(get_map);
                        irc.SendReply(e.Data, get_map);
                        Console.WriteLine(".");
                    }
                    else
                    {
                        if (message != "!r")
                            irc.SendReply(e.Data, "Currently works with 4 and 7 only.");

                        Log.Write("Keys error.");
                    }
                } //secret command :)
                else if (message.StartsWith("!helpmeplz"))
                {
                    Log.Write("===================REPORT====================");
                    Log.Write(e.Data.RawMessage);
                    Log.Write("===================REPORT====================");
                    Log.Report(e.Data.RawMessage);
                    irc.SendReply(e.Data, "Report sent. Thanks for help!");
                }
                else if (message.StartsWith("!acc") && _acc)
                {
                    string[] args = message.Substring(e.Data.Message.IndexOf(" ") + 1).Split(' ');
                    if (args.Length != 2)
                    {
                        irc.SendReply(e.Data, "Too few arguments. Example: !acc 96,12 876159");
                        return;
                    }

                    string map_id = null;
                    //find user's /np
                    for (int i = 0; i < users.Count; i++)
                    {
                        if (users[i] == e.Data.Nick)
                        {
                            try
                            {
                                map_id = last_map[i];
                            }
                            catch (ArgumentOutOfRangeException ex)
                            {
                                Log.Write("" + ex);
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
                        Log.Write($"{acc}");
                        score = Convert.ToDouble(args[1]);
                        Log.Write($"{score}");
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
                        Log.Write("Wrong input.");
                        irc.SendReply(e.Data, "Wrong input. Usage: !acc [acc] [score]");
                    }
                    else
                    {
                        MapInfo m = new MapInfo(map_id);
                        if (m.mode == "3")
                        {
                            string output = $"{m.artist} - {m.title} [{m.version}] | {Data.Calculate(m.od, m.stars, m.obj, acc, score)}pp for {acc}%";
                            Log.Write(output);
                            irc.SendReply(e.Data, output);
                        }
                        else if (m.mode == "0" || m.mode == "1" || m.mode == "2")
                        {
                            irc.SendReply(e.Data, "Mania mode required.");
                            Log.Write("Error: (166) Wrong mode.");
                        }
                        else
                        {
                            irc.SendReply(e.Data, "Please listen to mania song then do /np.");
                            Log.Write("Error: 171 m.mode is \"" + m.mode + "\"");
                            Console.WriteLine("Please listen to mania song then do /np.");
                        }
                    }
                }
            }
            catch (Exception ex)
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
            if (!e.Data.RawMessage.Contains("QUIT")
                && !e.Data.RawMessage.Contains("JOIN")
                && e.Data.RawMessage != null
                && !e.Data.RawMessage.Contains("PRIVMSG")
                && !e.Data.RawMessage.Contains("PING")
                && !e.Data.RawMessage.Contains("PONG"))
            {
                Log.Write(e.Data.RawMessage);
                Console.WriteLine(e.Data.RawMessage);
            }

        }

        public static void OnDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected.");
            Log.Write("Disconnected.");
            Thread.Sleep(5000);
            Connect(username, password);
        }

        public static void Connect(string username, string pass, int port = 6667, string server = "irc.ppy.sh")
        {
            try
            {
                irc.Connect(server, port);
                irc.Login(username, username, 0, "", pass);
                new Thread(new ThreadStart(ReadCommands)).Start();
                irc.Listen();
            }
            catch (ConnectionException ex)
            {
                Console.WriteLine("Couldn't connect, reason: " + ex);
                Log.Write("Error: " + ex);
                Thread.Sleep(10000);
                Connect(username, pass, port, server);
            }
        }

        static void Main(string[] args)
        {
            irc.Encoding = Encoding.UTF8;
            Log.Init();
            Data.LoadSettings();
            Log.Write(username);
            Log.Write(password);
            Log.Write(Data.ApiKey);
            Console.Title = "Amatsu!";
            irc.SendDelay = 200;
            irc.ActiveChannelSyncing = true;
            irc.OnQueryMessage += new IrcEventHandler(OnQueryMessage);
            irc.OnError += new ErrorEventHandler(OnError);
            irc.OnRawMessage += new IrcEventHandler(OnRawMessage);
            irc.OnQueryAction += new ActionEventHandler(OnQueryAction);
            irc.OnDisconnected += new EventHandler(OnDisconnected);
            users.Add("-_Alexmal_-");
            last_map.Add("425725");
            Connect(username, password);
        }

        public static void ReadCommands()
        {
            while (true)
            {
                string cmd = Console.ReadLine();

                if (cmd.StartsWith("/test "))
                {
                    irc.SendMessage(SendType.Message, "-_Alexmal_-", "Test command initiated.");
                }
                else if (cmd.StartsWith("/clear "))
                {
                    Console.Clear();
                }
                else if (cmd.StartsWith("/r "))
                {
                    _r = !_r;
                    Console.WriteLine("Changed _r to " + _r);
                }
                else if (cmd.StartsWith("/acc "))
                {
                    _acc = !_acc;
                    Console.WriteLine("Changed _acc to " + _acc);
                }
                else if (cmd.StartsWith("/np "))
                {
                    _np = !_np;
                    Console.WriteLine("Changed _np to " + _np);
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
