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
        private static List<string> _7keysDT = File.ReadAllLines("7keysDT.txt").ToList();
        private static List<string> _4keysDT = File.ReadAllLines("4keysDT.txt").ToList();

        public static void LoadSettings()
        {
            try
            {
                var data = File.ReadLines("account.txt").ToList();
                ApiKey = data[0].Split(':')[1].Replace("\r", "");
                Program.username = data[1].Split(':')[1].Replace("\r", "");
                Program.password = data[2].Split(':')[1].Replace("\r", "");
                data.Clear();

            }
            catch (FileNotFoundException ignore)
            {
                File.WriteAllText("account.txt", "Your osu!api key (osu.ppy.sh/p/api):abcd1234\r\nYour username (osu.ppy.sh/p/irc):-_Alexmal_-\r\nYour password (osu.ppy.sh/p/irc):my_password");
                LoadSettings();
                Console.WriteLine("Please, edit accounts.txt with your account settings.");
            }
        }

        public static string GetMap(string username, List<double> pp, string keys)
        {
            try
            {
                double ppWindow;
                double successRateWindow;
                var rand = new Random();
                var strings = new List<string>();
                if (keys == "7")
                    strings = _7keys;
                else if (keys == "4")
                    strings = _4keys;
                var scores = new List<string>();

                if (!Players.ContainsKey(username))
                {
                    strings = new List<string>();
                    if (keys == "7")
                    {
                        strings = _7keys;
                    }
                    else if (keys == "4")
                    {
                        strings = _4keys;
                    }
                    scores = new List<string>();
                    if (!Players.ContainsKey(username))
                    {
                        foreach (string str in strings)
                        {
                            double successRate = Convert.ToDouble(str.Split(',')[5].Replace('.', ','));
                            string score = str.Split(',')[1];
                            if (successRate > 0.25 && Convert.ToDouble(score) >= pp[1] && Convert.ToDouble(score) <= (pp[0] + pp[0]/10) && !string.IsNullOrWhiteSpace(score))
                            {
                                scores.Add(str);
                            }
                        }
                        Players.Add(username, new Player(username, scores));
                    }
                }
                else if (Players[username].Scoreslist.Count < 2)
                {
                    foreach (string str in strings)
                    {
                        double successRate = Convert.ToDouble(str.Split(',')[5].Replace('.', ','));
                        string score = str.Split(',')[1];
                        if (successRate > 0.18  && Convert.ToDouble(score) >= pp[1] && Convert.ToDouble(score) <= pp[0] && !string.IsNullOrWhiteSpace(score))
                        {
                            scores.Add(str);
                        }
                    }
                    scores.Shuffle();
                    Players[username].Scoreslist = scores;
                    Players[username].SuccessRateMod += 1;
                }
                if (Players[username].Scoreslist.Count > 0)
                {
                    Log.Write($"Scorlist count: {Players[username].Scoreslist.Count}");
                    int n = rand.Next(0, Players[username].Scoreslist.Count);
                    scores = Players[username].Scoreslist;
                    string map_id = scores[n].Split(',')[3];
                    string pp98 = scores[n].Split(',')[2];
                    string pp95 = scores[n].Split(',')[1];
                    string pp92 = scores[n].Split(',')[0];
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
                            var btm = JsonConvert.DeserializeObject<Beatmaps>(result.Substring(1, result.Length - 2));

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
                else
                {
                    return "No maps for you. :(";
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.ToString());
                Console.WriteLine(ex);
                return "Error occured.";
            }

        }

        public static string GetMapDiff(string username, double difficulty, string keys)
        {
            try
            {
                var rand = new Random();
                double starRatingWindow = 0.05 + difficulty / 20;
                var strings = new List<string>();
                if (keys == "7")
                {
                    strings = _7keys;
                }
                else if (keys == "4")
                {
                    strings = _4keys;
                }
                var scores = new List<string>();
                if (!Players.ContainsKey(username))
                {
                    foreach (string str in strings)
                    {
                        var star_rating = str.Split(',')[4].Replace('.', ',');
                        if (Convert.ToDouble(star_rating) >= difficulty - starRatingWindow && Convert.ToDouble(star_rating) <= difficulty + starRatingWindow && !string.IsNullOrWhiteSpace(star_rating))
                        {
                            scores.Add(str);
                        }
                    }
                    Players.Add(username, new Player(username, scores));
                }
                else if (Players[username].Scoreslist.Count < 2)
                {
                    foreach (string str in strings)
                    {
                        var star_rating = str.Split(',')[4].Replace('.', ',');
                        if (Convert.ToDouble(star_rating) >= difficulty - starRatingWindow && Convert.ToDouble(star_rating) <= difficulty + starRatingWindow / 2.5 && !string.IsNullOrWhiteSpace(star_rating))
                        {
                            scores.Add(str);
                        }
                    }
                    scores.Shuffle();
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
                        var btm = JsonConvert.DeserializeObject<Beatmaps>(result.Substring(1, result.Length - 2));

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

        public static string GetMapMinMaxDiff(string username, double minDifficulty, double maxDifficulty, string keys)
        {
            try
            {
                var rand = new Random();
                var strings = new List<string>();
                if (keys == "7")
                {
                    strings = _7keys;
                }
                else if (keys == "4")
                {
                    strings = _4keys;
                }
                var scores = new List<string>();
                if (!Players.ContainsKey(username))
                {
                    foreach (string str in strings)
                    {
                        var star_rating = str.Split(',')[4].Replace('.', ',');
                        if (Convert.ToDouble(star_rating) >= minDifficulty && Convert.ToDouble(star_rating) <= maxDifficulty && !string.IsNullOrWhiteSpace(star_rating))
                        {
                            scores.Add(str);
                        }
                    }
                    Players.Add(username, new Player(username, scores));
                }
                else if (Players[username].Scoreslist.Count < 2)
                {
                    foreach (string str in strings)
                    {
                        var star_rating = str.Split(',')[4].Replace('.', ',');
                        if (Convert.ToDouble(star_rating) >= minDifficulty && Convert.ToDouble(star_rating) <= maxDifficulty && !string.IsNullOrWhiteSpace(star_rating))
                        {
                            scores.Add(str);
                        }
                    }
                    scores.Shuffle();
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
                        var btm = JsonConvert.DeserializeObject<Beatmaps>(result.Substring(1, result.Length - 2));

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

        public static void CreatePlayer(string username, double pp, string keys)
        {
            double formula;
            double successRateWindow;
            var rand = new Random();
            if (pp > 30)
            {
                formula = pp / 20;
                successRateWindow = pp / 100;
            }
            else
            {
                formula = 5;
                successRateWindow = 0;
            }
            var strings = new List<string>();
            if (keys == "7")
            {
                strings = _7keys;
            }
            else if (keys == "4")
            {
                strings = _4keys;
            }
            var scores = new List<string>();
            if (!Players.ContainsKey(username))
            {
                foreach (string str in strings)
                {
                    double successRate = Convert.ToDouble(str.Split(',')[5].Replace('.', ','));
                    string score = str.Split(',')[1];
                    if (successRate + successRateWindow > 0.25 && Convert.ToDouble(score) >= pp - formula && Convert.ToDouble(score) <= pp + formula && !string.IsNullOrWhiteSpace(score))
                    {
                        scores.Add(str);
                    }
                }
                Players.Add(username, new Player(username, scores));
            }
        }

        public static double Calculate(double od, double starRating, double objectCount, double acc, double scoreValue = 0)
        {
            try
            {

                od = 64 - (3 * od);
                double strainMult = 1;

                strainMult = Math.Pow(5.0 *Math.Max(1.0f, starRating / 0.2) - 4.0, 2.2) / 135.0;

                strainMult *= 1 + 0.1f * Math.Min(1.0, objectCount / 1500.0);

                if (acc == 98 && scoreValue == 0)
                {
                    scoreValue = 900000;
                }
                else if (acc == 95 && scoreValue == 0)
                {
                    scoreValue = 800000;
                }
                else if (acc == 92 && scoreValue == 0)
                {
                    scoreValue = 700000;
                }

                if (scoreValue <= 500000)
                {
                    strainMult = 0;
                }
                else if (scoreValue <= 600000)
                {
                    strainMult *= (scoreValue - 500000) / 100000.0 * 0.3;
                }
                else if (scoreValue <= 700000)
                {
                    strainMult *= 0.3 + (scoreValue - 600000) / 100000.0 * 0.25;
                }
                else if (scoreValue <= 800000)
                {
                    strainMult *= 0.55 + (scoreValue - 700000) / 100000 * 0.20;
                }
                else if (scoreValue <= 900000)
                {
                    strainMult *= 0.75 + (scoreValue - 800000) / 100000 * 0.15;
                }
                else
                {
                    strainMult *= 0.90 + (scoreValue - 900000) / 100000 * 0.1;
                }

                double AccValue = Math.Max(0, 0.2 - ((od - 34) * 0.006667)) * strainMult * Math.Pow((Math.Max(0.0, (scoreValue - 960000)) / 40000.0), 1.1);
                double final_output = Math.Round(Math.Pow(Math.Pow(strainMult,1.1) + Math.Pow(AccValue,1.1),1.0/1.1) * 0.8);
                Log.Write($"(Data.Calculate) {final_output}");
                return final_output;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Log.Write($"Error: {ex}");
                Log.Write($"OD: {od} STARS: {starRating} OBJECT COUNT: {objectCount}, ACC: {acc}");
                return -1;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            var rand = new Random();
            while (n > 1)
            {
                int k = (rand.Next(0, n) % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
