using System;
using RestSharp;
using Newtonsoft.Json;

namespace Amatsu
{
    class MapInfo
    {
        public Double od { get; set; }
        public Double obj { get; set; }
        public Double stars { get; set; }
        public string mode { get; set; }
        public string artist { get; set; }
        public string title { get; set; }
        public string version { get; set; }

        public MapInfo(string map_id)
        {
            try
            {
                RestClient client = new RestClient("https://osu.ppy.sh/api/");
                RestRequest request = new RestRequest($"get_beatmaps?b={map_id}&k={Data.ApiKey}");
                request.Timeout = 5000;
                client.Timeout = 5000;
                IRestResponse response = client.Execute(request);
                string result = response.Content;
                if (result.Length > 2)
                {
                    Beatmaps btm = JsonConvert.DeserializeObject<Beatmaps>(result.Substring(1, result.Length - 2));
                    od = Convert.ToDouble(btm.diff_overall.Replace('.', ','));
                    obj = Convert.ToDouble(Osu.Combo(map_id));
                    stars = Convert.ToDouble(btm.difficultyrating.Replace('.', ','));
                    mode = btm.mode;
                    artist = btm.artist;
                    title = btm.title;
                    version = btm.version;
                }
                else
                {
                    mode = "-1";
                }
            }
            catch (Exception ex)
            {
                Log.Write($"ERROR: {ex}");
            }
        }
    }
}
