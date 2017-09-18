using System;
using System.Linq;
using System.IO;
using RestSharp;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Amatsu
{
    static class Data
    {
        public static Dictionary<string, Player> Players = new Dictionary<string, Player>();
        public static string ApiKey = "Your api key";
        private static List<string> _7keys = File.ReadAllLines("7keys.txt").ToList();
        private static List<string> _4keys = File.ReadAllLines("4keys.txt").ToList();

        public static void LoadSettings()
        {
            try
            {
                List<string> data = File.ReadLines("account.txt").ToList();
                ApiKey = data[0].Split(':')[1].Replace("\r","");
                Program.username = data[1].Split(':')[1].Replace("\r", "");
                Program.password = data[2].Split(':')[1].Replace("\r", "");
                data.Clear();

            }
            catch(FileNotFoundException ignore)
            {
                File.WriteAllText("account.txt", "Your osu!api key (osu.ppy.sh/p/api):abcd1234\r\nYour username (osu.ppy.sh/p/irc):-_Alexmal_-\r\nYour password (osu.ppy.sh/p/irc):my_password");
                LoadSettings();
                Console.WriteLine("Please, edit accounts.txt with your account settings.");
            }
        }

        public static string GetMap(string username, Double _pp, string _keys)
        {
            try
            {
                var rand = new Random();
                Double formula = _pp / 20;
                List<string> strings = new List<string>();
                if (_keys == "7")
                    strings = _7keys;
                else if (_keys == "4")
                    strings = _4keys;
                List<string> scores = new List<string>();

                if (!Players.ContainsKey(username))
                {
                    foreach (string str in strings)
                    {
                        Double successRate = Convert.ToDouble(str.Split(',')[5].Replace('.', ','));
                        string score = str.Split(',')[1];
                        if (successRate > 0.36 && Convert.ToDouble(score) >= _pp - formula && Convert.ToDouble(score) <= _pp + formula && !string.IsNullOrWhiteSpace(score))
                        {
                            scores.Add(str);
                        }
                    }
                    Players.Add(username,new Player(username,scores));
                }
                else if (Players[username].Scoreslist.Count == 0)
                {
                    foreach (string str in strings)
                    {
                            Double successRate = Convert.ToDouble(str.Split(',')[5].Replace('.', ','));
                            string score = str.Split(',')[1];
                            if (successRate > 0.36 && Convert.ToDouble(score) >= _pp - formula && Convert.ToDouble(score) <= _pp + formula && !string.IsNullOrWhiteSpace(score))
                            {
                                scores.Add(str);
                            }
                    }
                    Players[username].Scoreslist = scores;
                }

                int n = rand.Next(0, Players[username].Scoreslist.Count);
                scores = Players[username].Scoreslist;
                string map_id = scores[n].Split(',')[3];
                string pp98 = scores[n].Split(',')[2];
                string pp95 = scores[n] .Split(',')[1];
                string pp92 = scores[n].Split(',')[0];
                scores.RemoveAt(n);

                RestClient client = new RestClient("https://osu.ppy.sh/api/");
                RestRequest request = new RestRequest($"get_beatmaps?k={ApiKey}&b={map_id}&m=3");
                client.Timeout = 5000;
                request.Timeout = 5000;
                IRestResponse response = client.Execute(request);
                scores.Clear();
                if (response.ResponseStatus != ResponseStatus.TimedOut)
                {
                    string result = response.Content;
                    if (result.Length > 2)
                    {
                        Beatmaps btm = JsonConvert.DeserializeObject<Beatmaps>(result.Substring(1, result.Length - 2));

                        string output = $"[https://osu.ppy.sh/b/{map_id} {btm.artist} - {btm.title} [{btm.version}]]  92%: {pp92}pp, 95%: {pp95}pp, 98%: {pp98}pp | {btm.bpm}bpm  {Math.Round(Convert.ToDouble(btm.difficultyrating.Replace('.', ',')), 2)}*";
                        return output;
                    }
                    else
                    {
                        return "Whoops! Looks like request failed. Try again.";
                    }
                }
                else
                {
                    return "Timed Out.";
                }
            }
            catch(Exception ex)
            {
                Log.Write(ex.ToString());
                Console.WriteLine(ex);
                return "Error occured.";
            }

        }

        public static string GetMapDiff(string username, Double difficulty, string keys)
        {
            try
            {
                var rand = new Random();
                Double formula = 0.05 + difficulty/20;
                List<string> strings = new List<string>();
                if (keys == "7")
                {
                    strings = _7keys;
                }
                else if (keys == "4")
                {
                    strings = _4keys;
                }
                List<string> scores = new List<string>();
                if (!Players.ContainsKey(username))
                {
                    foreach (string str in strings)
                    {
                        var star_rating = str.Split(',')[4].Replace('.', ',');
                        if (Convert.ToDouble(star_rating) >= difficulty - formula && Convert.ToDouble(star_rating) <= difficulty + formula && !string.IsNullOrWhiteSpace(star_rating))
                        {
                            scores.Add(str);
                        }
                    }
                    Players.Add(username, new Player(username, scores));
                }
                else if (Players[username].Scoreslist.Count == 0)
                {
                    foreach (string str in strings)
                    {
                        var star_rating = str.Split(',')[4].Replace('.', ',');
                        if (Convert.ToDouble(star_rating) >= difficulty - formula && Convert.ToDouble(star_rating) <= difficulty + formula / 2.5 && !string.IsNullOrWhiteSpace(star_rating))
                        {
                            scores.Add(str);
                        }
                    }
                    Players[username].Scoreslist = scores;
                }
                var n = rand.Next(0, scores.Count);
                var map_id = scores[n].Split(',')[3];
                var pp98 = scores[n].Split(',')[2];
                var pp95 = scores[n].Split(',')[1];
                var pp92 = scores[n].Split(',')[0];
                scores.RemoveAt(n);
                var client = new RestClient("https://osu.ppy.sh/api/");
                var request = new RestRequest($"get_beatmaps?k={ApiKey}&b={map_id}&m=3");
                client.Timeout = 5000;
                request.Timeout = 5000;
                var response = client.Execute(request);
                scores.Clear();
                if (response.ResponseStatus != ResponseStatus.TimedOut)
                {
                    string result = response.Content;
                    if (result.Length > 2)
                    {
                        Beatmaps btm = JsonConvert.DeserializeObject<Beatmaps>(result.Substring(1, result.Length - 2));

                        var output = $"[https://osu.ppy.sh/b/{map_id} {btm.artist} - {btm.title} [{btm.version}]]  92%: {pp92}pp, 95%: {pp95}pp, 98%: {pp98}pp | {btm.bpm}bpm  {Math.Round(Convert.ToDouble(btm.difficultyrating.Replace('.', ',')), 2)}*";
                        return output;
                    }
                    else
                    {
                        return "Whoops! Looks like request failed. Try again.";
                    }
                }
                else
                {
                    return "Timed Out.";
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.ToString());
                Console.WriteLine(ex);
                return "Error occured.";
            }
        }

        public static string GetMapMinMaxDiff(string username, Double minDifficulty, Double maxDifficulty, string keys)
        {
            try
            {
                var rand = new Random();
                List<string> strings = new List<string>();
                if (keys == "7")
                {
                    strings = _7keys;
                }
                else if (keys == "4")
                {
                    strings = _4keys;
                }
                List<string> scores = new List<string>();
                if (!Players.ContainsKey(username))
                {
                    foreach (string str in strings)
                    {
                        var star_rating = str.Split(',')[4].Replace('.', ',');
                        if (Convert.ToDouble(star_rating) >= minDifficulty && Convert.ToDouble(star_rating) <= maxDifficulty&& !string.IsNullOrWhiteSpace(star_rating))
                        {
                            scores.Add(str);
                        }
                    }
                    Players.Add(username, new Player(username, scores));
                }
                else if (Players[username].Scoreslist.Count == 0)
                {
                    foreach (string str in strings)
                    {
                        var star_rating = str.Split(',')[4].Replace('.', ',');
                        if (Convert.ToDouble(star_rating) >= minDifficulty && Convert.ToDouble(star_rating) <= maxDifficulty && !string.IsNullOrWhiteSpace(star_rating))
                        {
                            scores.Add(str);
                        }
                    }
                    Players[username].Scoreslist = scores;
                }
                var n = rand.Next(0, scores.Count);
                var map_id = scores[n].Split(',')[3];
                var pp98 = scores[n].Split(',')[2];
                var pp95 = scores[n].Split(',')[1];
                var pp92 = scores[n].Split(',')[0];
                scores.RemoveAt(n);
                var client = new RestClient("https://osu.ppy.sh/api/");
                var request = new RestRequest($"get_beatmaps?k={ApiKey}&b={map_id}&m=3");
                client.Timeout = 5000;
                request.Timeout = 5000;
                var response = client.Execute(request);
                scores.Clear();
                if (response.ResponseStatus != ResponseStatus.TimedOut)
                {
                    string result = response.Content;
                    if (result.Length > 2)
                    {
                        Beatmaps btm = JsonConvert.DeserializeObject<Beatmaps>(result.Substring(1, result.Length - 2));

                        var output = $"[https://osu.ppy.sh/b/{map_id} {btm.artist} - {btm.title} [{btm.version}]]  92%: {pp92}pp, 95%: {pp95}pp, 98%: {pp98}pp | {btm.bpm}bpm  {Math.Round(Convert.ToDouble(btm.difficultyrating.Replace('.', ',')), 2)}*";
                        return output;
                    }
                    else
                    {
                        return "Whoops! Looks like request failed. Try again.";
                    }
                }
                else
                {
                    return "Timed Out.";
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.ToString());
                Console.WriteLine(ex);
                return "Error occured.";
            }
        }

        public static Double Calculate(Double od, Double stars, Double obj, Double acc, Double _scr = 0)
        {
            try
            {
                od = 64 - (3 * od);
                Double strainMult = 1;

                if (acc == 98 && _scr == 0)
                {
                    _scr = 900000;
                }
                else if (acc == 95 && _scr == 0)
                {
                    _scr = 800000;
                }
                else if (acc == 92 && _scr == 0)
                {
                    _scr = 700000;
                }

                if (_scr < 500000)
                {
                    strainMult = _scr / 500000 * 0.1;
                }
                else if (_scr < 600000)
                {
                    strainMult = (_scr - 500000) / 100000 * 0.2 + 0.1;
                }
                else if (_scr < 700000)
                {
                    strainMult = (_scr - 600000) / 100000 * 0.35 + 0.3;
                }
                else if (_scr < 800000)
                {
                    strainMult = (_scr - 700000) / 100000 * 0.2 + 0.65;
                }
                else if (_scr < 900000)
                {
                    strainMult = (_scr - 800000) / 100000 * 0.1 + 0.85;
                }
                else
                {
                    strainMult = (_scr - 900000) / 100000 * 0.05 + 0.95;
                }

                Double StrainBase = (Math.Pow(5 * Math.Max(1, stars / 0.0825) - 4, 3) / 110000) * (1 + 0.1 * Math.Min(1, obj / 1500));
                Double AccValue = Math.Pow((150 / od) * Math.Pow(acc / 100, 16),1.8) * 2.5 * Math.Min(1.15,Math.Pow(obj / 1500, 0.3));
                Double fo0 = Math.Pow(AccValue, 1.1);
                Double fo1 = Math.Pow(StrainBase * strainMult, 1.1);
                Double final_output = Math.Round(Math.Pow(fo0 + fo1,1 / 1.1) * 1.1);
                Log.Write($"(Data.Calculate) fo0 {fo0} fo1 {fo1} StrainBase {StrainBase} AccValue {AccValue} / OD: {od} STARS: {stars} OBJECT COUNT: {obj}, ACC: {acc}");
                Log.Write($"(Data.Calculate) {final_output}");
                return final_output;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Log.Write($"Error: {ex}");
                Log.Write($"OD: {od} STARS: {stars} OBJECT COUNT: {obj}, ACC: {acc}");
                return -1;
            }
        }
    }
}
