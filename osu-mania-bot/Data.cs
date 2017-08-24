using System;
using System.Linq;
using System.IO;
using RestSharp;
using Newtonsoft.Json;

namespace Amatsu
{
    class Data
    {
        private static string _api = "7a64ea6efd955698cc9a26999daf640504a319d4";
        public static string GetMap(Double _pp, string _keys)
        {
            try
            {
                Random rand = new Random();
                Double formula = _pp / 10;
                StreamReader reader = new StreamReader(_keys + "keys.txt");
                string[] strings = reader.ReadToEnd().Split('\n');
                string[] scores = new string[2100];
                for (int i = 0; i < strings.Length - 1; i++)
                {
                    string score = strings[i].Substring(strings[i].IndexOf(',')+1);
                    score = score.Remove(score.IndexOf(','));
                    if(Convert.ToDouble(score)>=_pp-formula && Convert.ToDouble(score) <= _pp + formula && score !=null)
                    {
                        scores[scores.Count(s => s != null)] = strings[i];
                    }
                }
                int n = rand.Next(0, scores.Count(s => s != null && s!="\r\n" && s!="\r" && s!="\n") - 1);
                string map_id = scores[n];
                string pp98 = scores[n].Remove(scores[n].LastIndexOf(','));
                string pp95 = pp98.Remove(pp98.LastIndexOf(','));
                string pp92 = pp95.Remove(pp95.LastIndexOf(','));
                pp95 = pp95.Substring(pp95.LastIndexOf(',')+1);
                pp98 = pp98.Substring(pp98.LastIndexOf(',')+1);
                map_id = map_id.Substring(map_id.LastIndexOf(',') + 1);
                
                RestClient client = new RestClient("https://osu.ppy.sh/api/");
                RestRequest request = new RestRequest($"get_beatmaps?k={_api}&b={map_id}&m=3");
                client.Timeout = 5000; request.Timeout = 5000;
                IRestResponse response = client.Execute(request);

                string result = response.Content;
                Beatmaps btm = JsonConvert.DeserializeObject<Beatmaps>(result.Substring(1,result.Length-2));

                string output = $"[https://osu.ppy.sh/b/{map_id} {btm.artist} - {btm.title}]  92%: {pp92}pp, 95%: {pp95}pp, 97%: {pp98}pp | {btm.bpm}bpm  {Math.Round(Convert.ToDouble(btm.difficultyrating.Replace('.',',')),2)}*";
                return output;
            }
            catch(Exception ex)
            {
                Log.Write(ex.ToString());
                Console.WriteLine(ex);
                return "Error occured.";
            }

        }

        public static Double Calculate(Double od, Double stars, Double obj, Double acc)
        {
            try
            {
                Double strainMult = 1;
                if (acc == 98) { strainMult = 0.95; }
                else if (acc == 95) { strainMult = 0.85; }
                else if (acc == 92) { strainMult = 0.65; }
                Double StrainBase = (Math.Pow(5 * Math.Max(1, stars / 0.0825) - 4, 3) / 110000) * (1 + 0.1 * Math.Min(1, obj / 1500));
                Double AccValue = Math.Pow(150 / od * Math.Pow(acc / 100, 16), 1.8) * 2.5 * Math.Min(Math.Pow(obj / 1500, 0.3), 1.15);
                Double fo0 = Math.Pow(AccValue, 1.1);
                Double fo1 = Math.Pow(StrainBase * strainMult, 1.1);
                Double final_output = Math.Pow(fo0 + fo1, Math.Round(1 / 1.1, 2)) * 1.1;

                return final_output;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Log.Write($"Error: {ex}");
                return -1;
            }
        }
    }
    public class Beatmaps
    {
        public string beatmapset_id { get; set; }
        public string beatmap_id { get; set; }
        public string approved { get; set; }
        public string total_length { get; set; }
        public string hit_length { get; set; }
        public string version { get; set; }
        public string file_md5 { get; set; }
        public string diff_size { get; set; }
        public string diff_overall { get; set; }
        public string diff_approach { get; set; }
        public string diff_drain { get; set; }
        public string mode { get; set; }
        public object approved_date { get; set; }
        public string last_update { get; set; }
        public string artist { get; set; }
        public string title { get; set; }
        public string creator { get; set; }
        public string bpm { get; set; }
        public string source { get; set; }
        public string tags { get; set; }
        public string genre_id { get; set; }
        public string language_id { get; set; }
        public string favourite_count { get; set; }
        public string playcount { get; set; }
        public string passcount { get; set; }
        public string max_combo { get; set; }
        public string difficultyrating { get; set; }
    }
}
