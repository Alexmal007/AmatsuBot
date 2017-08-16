using System;
using RestSharp;
using Newtonsoft.Json;

namespace osu_mania_bot
{
    class Osu
    {
        private static string api = "osu!api key";

        public static Double GetAveragePP(string username)
        {
            Double _pp = 0;
            RestClient client = new RestClient("https://osu.ppy.sh/api");
            RestRequest request = new RestRequest($"/get_user_best?u={username}&k={api}&limit=10&m=3");
            IRestResponse response = client.Execute(request);
            string result = response.Content;
            if (result.Length > 2 && !result.Contains("error"))
            {
                UserBest[] usb = JsonConvert.DeserializeObject<UserBest[]>(result);
                for(int i = 0; i<usb.Length; i++)
                {
                    _pp = _pp + Convert.ToDouble(usb[i].pp.Replace('.',','));
                }
                _pp = _pp / usb.Length;
                return _pp;
            }
            else
            {
                return -1;
            }
        }

        public static string Calculate(Double acc, string map_id)
        {
            try
            {
                RestClient client = new RestClient("https://osu.ppy.sh/api/");
                RestRequest request = new RestRequest($"get_beatmaps?k={api}&b={map_id}");
                IRestResponse response = client.Execute(request);
                string result = response.Content;
                if (result.Length > 2)
                {
                    Beatmaps btm = JsonConvert.DeserializeObject<Beatmaps>(result.Substring(1, result.Length - 2));
                    if(btm.mode != "3")
                    {
                        string str = "Mode Error.";
                        return str;
                    }
                    Double od = Convert.ToDouble(btm.diff_overall);
                    Double stars = Convert.ToDouble(btm.difficultyrating.Replace('.',','));
                    Double obj = Convert.ToDouble(combo(map_id));
                    Double strainMult = 1;
                    if (acc == 98) { strainMult = 0.95; }
                    else if (acc == 95) { strainMult = 0.85; }
                    else if (acc == 92) { strainMult = 0.65; }
                    Double StrainBase = (Math.Pow(5 * Math.Max(1, stars / 0.0825) - 4, 3) / 110000) * (1 + 0.1 * Math.Min(1, obj / 1500));
                    Double AccValue = Math.Pow(150 / od * Math.Pow(acc / 100, 16), 1.8) * 2.5 * Math.Min(Math.Pow(obj / 1500, 0.3), 1.15);
                    Double fo0 = Math.Pow(AccValue, 1.1);
                    Double fo1 = Math.Pow(StrainBase * strainMult, 1.1);
                    Double final_output = Math.Round(Math.Pow(fo0 + fo1, Math.Round(1 / 1.1, 2)) * 1.1);

                    string output = Convert.ToString(final_output);
                    return output;
                }
                else
                {
                    return "API Error occuried.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Log.Write($"Error: {ex}");
                return "Error.";
            }
        }

        private static string combo(string map_id)
        {
            try
            {
                string max_combo;
                RestClient client = new RestClient("https://osu.ppy.sh/api/");
                RestRequest request = new RestRequest($"get_scores?b={map_id}&k={api}&m=3&limit=1");
                IRestResponse response = client.Execute(request);
                string result = response.Content;
                if (result.Length > 2)
                {
                    Scores scr = JsonConvert.DeserializeObject<Scores>(result.Substring(1, result.Length - 2));
                    max_combo = Convert.ToString(Convert.ToInt16(scr.count300) + Convert.ToInt16(scr.count100) + Convert.ToInt16(scr.count50) + Convert.ToInt16(scr.countmiss) + Convert.ToInt16(scr.countgeki));
                    return max_combo;
                }
                else
                {
                    return "Error.";
                }
            }
            catch (Exception ex)
            {
                Log.Write("Error: " + ex);
                return "Error.";
            }
        }

    }

    public class UserBest
    {
        public string beatmap_id { get; set; }
        public string score { get; set; }
        public string maxcombo { get; set; }
        public string count50 { get; set; }
        public string count100 { get; set; }
        public string count300 { get; set; }
        public string countmiss { get; set; }
        public string countkatu { get; set; }
        public string countgeki { get; set; }
        public string perfect { get; set; }
        public string enabled_mods { get; set; }
        public string user_id { get; set; }
        public string date { get; set; }
        public string rank { get; set; }
        public string pp { get; set; }
    }

    public class Scores
    {
        public string score_id { get; set; }
        public string score { get; set; }
        public string username { get; set; }
        public string maxcombo { get; set; }
        public string count50 { get; set; }
        public string count100 { get; set; }
        public string count300 { get; set; }
        public string countmiss { get; set; }
        public string countkatu { get; set; }
        public string countgeki { get; set; }
        public string perfect { get; set; }
        public string enabled_mods { get; set; }
        public string user_id { get; set; }
        public string date { get; set; }
        public string rank { get; set; }
        public string pp { get; set; }
    }

}
