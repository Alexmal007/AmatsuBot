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
        public static string ApiKey = "Your api key";

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

        public static string GetMap(Double _pp, string _keys)
        {
            try
            {
                var rand = new Random();
                Double formula = _pp / 20;
                string[] strings = File.ReadAllLines(_keys + "keys.txt");
                List<string> scores = new List<string>();

                foreach(string str in strings)
                {
                    string score = str.Substring(str.IndexOf(',') + 1);
                    score = score = score.Substring(score.IndexOf(',') + 1);
                    score = score.Remove(score.IndexOf(','));
                    if (Convert.ToDouble(score) >= _pp - formula && Convert.ToDouble(score) <= _pp + formula && score != null)
                    {
                        scores.Add(str);
                    }
                }

                int n = rand.Next(0, scores.Count);

                string map_id = scores[n];
                string pp98 = scores[n].Remove(scores[n].LastIndexOf(','));
                string pp95 = pp98.Remove(pp98.LastIndexOf(','));
                string pp92 = pp95.Remove(pp95.LastIndexOf(','));
                pp95 = pp95.Substring(pp95.LastIndexOf(',') + 1);
                pp98 = pp98.Substring(pp98.LastIndexOf(',') + 1);
                map_id = map_id.Substring(map_id.LastIndexOf(',') + 1);
                
                RestClient client = new RestClient("https://osu.ppy.sh/api/");
                RestRequest request = new RestRequest($"get_beatmaps?k={ApiKey}&b={map_id}&m=3");
                client.Timeout = 5000;
                request.Timeout = 5000;
                IRestResponse response = client.Execute(request);

                string result = response.Content;
                if (result.Length > 2)
                {
                    Beatmaps btm = JsonConvert.DeserializeObject<Beatmaps>(result.Substring(1, result.Length - 2));

                    string output = $"[https://osu.ppy.sh/b/{map_id} {btm.artist} - {btm.title} [{btm.version}]]  92%: {pp92}pp, 95%: {pp95}pp, 98%: {pp98}pp | {btm.bpm}bpm  {Math.Round(Convert.ToDouble(btm.difficultyrating.Replace('.', ',')), 2)}*";
                    return output;
                }
                else
                {
                    return "Timed out.";
                }
            }
            catch(Exception ex)
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
