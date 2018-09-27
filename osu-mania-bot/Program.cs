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
        private static string _announce = null;

        public static void OnQueryAction(object sender, ActionEventArgs e)
        {
            Log.Write(e.Data.RawMessage);
            Console.WriteLine(e.Data.Message);


            if (e.Data.RawMessage.Contains("https://osu.ppy.sh/b/") || e.Data.RawMessage.Contains("http://osu.ppy.sh/b/"))
            {
                try
                {
                    bool checkpoint = false;
                    Console.WriteLine(e.Data.Nick + ": " + e.Data.Message);
                    string map_id = e.Data.Message.Substring(e.Data.Message.IndexOf("sh/b/") + 5);

                    map_id = map_id.Remove(map_id.IndexOf(" "));
                    if (map_id.Contains("?"))
                        map_id = map_id.Remove(map_id.IndexOf("?"));


                    Console.WriteLine("Map ID: " + map_id);

                    for (int i = 0; i < users.Count; i++)
                    {
                        if (users[i] == (e.Data.Nick))
                        {
                            last_map[i] = map_id;
                            checkpoint = true;
                        }
                    }

                    if (!checkpoint)
                    {
                        users.Add(e.Data.Nick);
                        last_map.Add(map_id);
                        if (_announce != null)
                        {
                            irc.SendReply(e.Data, _announce);
                        }
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

        public static void OnQueryMessage(object sender, IrcEventArgs e)
        {
            Console.WriteLine(e.Data.Nick + ":" + e.Data.Message);
            Log.Write(e.Data.RawMessage);
            if (!users.Contains(e.Data.Nick))
            {
                users.Add(e.Data.Nick);
                last_map.Add("1023965");
                if (_announce != null)
                {
                    irc.SendReply(e.Data, _announce);
                }
            }
            if (e.Data.Message.StartsWith("!"))
            {
                var command = e.Data.Message.ToLower().Split(' ');

                if (command[0] == "!r")
                {   

                    if (command.Length > 1 && command[1] == "4")
                    {
                        var list = Osu.GetPP(e.Data.Nick);
                        Log.Write($"{list[0]} | {list[1]}");
                        if (list[0] == -1)
                        {
                            irc.SendReply(e.Data, "Request failed. Time for a break, maybe? :) Possible problems: You don't have enough scores in mania mode; Connection failed for some reason.");
                        }
                        else
                        {
                            var get_map = Data.GetMap(e.Data.Nick, list, "4");
                            Log.Write(get_map);
                            irc.SendReply(e.Data, get_map);
                            Console.WriteLine("~Reply sent.");
                        }
                    }
                    else if (command.Length > 1 && command[1] == "7")
                    {
                        var pp = Osu.GetPP(e.Data.Nick);
                        Log.Write($"{pp[0]}");
                        if (pp[0] == -1)
                        {
                            irc.SendReply(e.Data, "Request failed. Time for a break, maybe? :) Possible problems: You don't have enough scores in mania mode; Connection failed for some reason.");
                        }
                        else
                        {
                            var get_map = Data.GetMap(e.Data.Nick, pp, "7");
                            Log.Write(get_map);
                            irc.SendReply(e.Data, get_map);
                            Console.WriteLine("~Reply sent.");
                        }
                    }
                    else if (command.Length == 1)
                    {
                        Console.WriteLine("Too few arguments.");
                        Log.Write("Too few arguments.");
                        irc.SendReply(e.Data, "Too few aruments. Command usage: !r [keys]. Example: !r 4");
                    }
                    else
                    {
                        Log.Write("4 or 7 keys required.");
                        irc.SendReply(e.Data, "Due to lack of maps I can only work with 4 and 7 keys right now.");
                    }
                }
                else if (command[0] == "!acc")
                {
                    if (command.Length == 3)
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
                                catch (ArgumentOutOfRangeException ex)
                                {
                                    Log.Write("" + ex);
                                    Console.WriteLine("Error (ArgumentOutOfRangeException)");
                                    irc.SendReply(e.Data, "Send /np first. (map must be ranked)");
                                }
                            }
                        }
                        Double acc;
                        Double score;
                        try
                        {
                            acc = Convert.ToDouble(command[1].Replace('.', ','));
                            Log.Write($"{acc}");
                            score = Convert.ToDouble(command[2]);
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
                                irc.SendReply(e.Data, "Please, listen to mania song then do /np.");
                                Log.Write("Error: 171 m.mode is \"" + m.mode + "\"");
                                Console.WriteLine("Please, listen to mania song then do /np.");
                            }
                        }
                    }
                    else
                    {
                        Log.Write("Too few arguments.");
                        Console.WriteLine("Too few arguments.");
                        irc.SendReply(e.Data, "Too few arguments. Usage: !acc [accuracy] [score]. Example: !acc 96.04 842098");
                    }
                }
                else if (command[0] == "!dif" || command[0] == "!diff")
                {
                    if (command.Length < 3)
                    {
                        Log.Write(e.Data.Nick + "// Too few arguments.");
                        Console.WriteLine("Too few arguments.");
                        irc.SendReply(e.Data, "Too few arguments. Usage: !diff [keys] [difficulty]. Example: !diff 4 3.55");
                    }
                    else if (command.Length >= 3)
                    {
                        try
                        {
                            var diff = Convert.ToDouble(command[2].Replace('.', ','));
                            var keys = command[1];
                            if (command[1] == "4" || command[1] == "7")
                            {
                                string get_map;
                                if (command.Length == 3)
                                {
                                    get_map = Data.GetMapDiff(e.Data.Nick, diff, keys);
                                }
                                else
                                {
                                    var diff2 = Convert.ToDouble(command[3].Replace('.', ','));
                                    get_map = Data.GetMapMinMaxDiff(e.Data.Nick, diff, diff2, command[1]);
                                }
                                Log.Write(get_map);
                                irc.SendReply(e.Data, get_map);
                                Console.WriteLine("~Reply sent.");
                            }
                            else
                            {
                                irc.SendReply(e.Data, "Works for 4 and 7 keys for now.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write($"ex");
                            Console.WriteLine(ex);
                            irc.SendReply(e.Data, "Error occuried. Wrong input?");
                        }
                    }
                }
                else if (command[0] == "!help" || command[0] == "!info")
                {
                    irc.SendReply(e.Data, "Hello, I'm Amatsu! And I can do some cute things for you. Forum thread can be found [https://osu.ppy.sh/forum/t/637171 here], commands are listed [https://github.com/Alexmal007/AmatsuBot/wiki here]");
                }
                else if (command[0] == "!coffee")
                {
                    irc.SendReply(e.Data, "[https://youtu.be/SpiPD_Ti_Bg Держи^^]");
                }
                else if (command[0] == "!sendreport")
                {
                    if (command.Length > 1)
                    {
                        irc.SendMessage(SendType.Message,username,$"(REPORT) {e.Data.Nick}: {command[1]}");
                        Log.Report($"{e.Data.Nick}: {command[1]}");
                        irc.SendReply(e.Data, "Report sent!");
                    }
                    else
                    {
                        irc.SendReply(e.Data, "Too few arguments. Usage: !report [report message].");
                    }
                }
                else
                {
                    irc.SendReply(e.Data, "Unknown command. All available commands are listed [https://github.com/Alexmal007/AmatsuBot/wiki here].");
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
            try
            {
                while (true)
                {
                    string cmd = Console.ReadLine();

                    if (cmd.StartsWith("/test"))
                    {
                        irc.SendMessage(SendType.Message, "-_Alexmal_-", "Test command initiated.");
                    }
                    else if (cmd.StartsWith("/clear"))
                    {
                        Console.Clear();
                    }

                    if (cmd.StartsWith("/announce"))
                    {
                        try
                        {
                            _announce = cmd.Substring(cmd.IndexOf(' ') + 1);
                            Console.WriteLine($"Set announce to \"{_announce}\"");
                        }
                        catch (Exception ex)
                        {
                            _announce = null;
                            Console.WriteLine("_announce set to null.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
