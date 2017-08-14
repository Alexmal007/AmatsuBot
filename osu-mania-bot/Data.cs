using System;
using System.Linq;
using System.IO;
using RestSharp;
using Newtonsoft.Json;

namespace osu_mania_bot
{
    class Data
    {
        private static string _api = "Your osu! api key";
        public static string GetMap(Double _pp, string _keys)
        {
            try
            {
                Random rand = new Random();
                Double formula = _pp / 10;
                StreamReader reader = new StreamReader(_keys + "k_table.txt");
                string[] strings = reader.ReadToEnd().Split('\r');
                string[] scores = new string[15000];
                for (int i = 0; i < strings.Length - 1; i++)
                {
                    string score = strings[i].Substring(strings[i].IndexOf(',')+1);
                    score = score.Remove(score.IndexOf(','));
                    if(Convert.ToDouble(score)>=_pp-formula && Convert.ToDouble(score) <= _pp + formula && score !=null)
                    {
                        scores[scores.Count(s => s != null)] = strings[i];
                    }
                }

                int n = rand.Next(0, scores.Count(s => s != null) - 1);
                string map_id = scores[n].Replace("\n", "");
                string pp97 = scores[n].Remove(scores[n].LastIndexOf(','));
                string pp95 = pp97.Remove(pp97.LastIndexOf(','));
                string pp92 = pp95.Remove(pp95.LastIndexOf(','));
                pp92 = pp92.Replace("\n", "");
                pp95 = pp95.Substring(pp95.LastIndexOf(',')+1);
                pp97 = pp97.Substring(pp97.LastIndexOf(',')+1);
                map_id = map_id.Substring(map_id.LastIndexOf(',') + 1);
                
                RestClient client = new RestClient("https://osu.ppy.sh/api/");
                RestRequest request = new RestRequest($"get_beatmaps?k={_api}&b={map_id}");
                IRestResponse response = client.Execute(request);
                string result = response.Content;
                Beatmaps btm = JsonConvert.DeserializeObject<Beatmaps>(result.Substring(1,result.Length-2));

                string output = $"[https://osu.ppy.sh/b/{map_id} {btm.artist} - {btm.title}]  92%: {pp92}pp, 95%: {pp95}pp, 97%: {pp97}pp | {btm.bpm}bpm  {Math.Round(Convert.ToDouble(btm.difficultyrating.Replace('.',',')),2)}*";
                return output;
            }
            catch(Exception ex)
            {
                Log.Write(ex.ToString());
                Console.WriteLine(ex);
                return "Error occured";
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
