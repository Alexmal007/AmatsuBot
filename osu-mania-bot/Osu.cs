using System;
using RestSharp;
using Newtonsoft.Json;

namespace osu_mania_bot
{
    class Osu
    {
        private static string api = "Your osu! api key";
        private static Double _pp = 0;

        public static Double GetAveragePP(string username)
        {
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

}
