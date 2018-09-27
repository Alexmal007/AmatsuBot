using System;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;

namespace Amatsu
{
    class Osu
    {
        public static List<double> GetPP(string username)
        {
            try
            {
                var client = new RestClient("https://osu.ppy.sh/api");
                var request = new RestRequest($"/get_user_best?u={username}&k={Data.ApiKey}&limit=15&m=3");
                client.Timeout = 5000;
                request.Timeout = 5000;
                var response = client.Execute(request);
                string result = response.Content;
                if (result.Length > 2 && !result.Contains("error"))
                {
                    var usb = JsonConvert.DeserializeObject<UserBest[]>(result);
                    double ppFirst = Convert.ToDouble(usb[0].pp.Replace('.', ','));
                    double ppLast = Convert.ToDouble(usb[usb.Length - 1].pp.Replace('.', ','));
                    var output = new List<double>();
                    output.Add(ppFirst);
                    output.Add(ppLast);
                    return output;
                }
                else
                {
                    Log.Write($"Request failed. Result length: {result.Length} / Result: {result}");
                    var output = new List<double>();
                    output.Add(-1);
                    output.Add(-1);
                    return output;
                }
            }
            catch (Exception ex)
            {
                Log.Write($"Error: {ex}");
                Console.WriteLine(ex);
                var output = new List<Double>();
                output.Add(-1);
                output.Add(-1);
                return output;
            }
        }


        public static string Calculate(double accuracy, double objectCount, double starRating, double odValue)
        {
            try
            {
                    double strainMult = 1;
                    if (accuracy == 98) { strainMult = 0.95; }
                    else if (accuracy == 95) { strainMult = 0.85; }
                    else if (accuracy == 92) { strainMult = 0.65; }
                    double StrainBase = (Math.Pow(5 * Math.Max(1, starRating / 0.0825) - 4, 3) / 110000) * (1 + 0.1 * Math.Min(1, objectCount / 1500));
                    double AccValue = Math.Pow(150 / odValue * Math.Pow(accuracy / 100, 16), 1.8) * 2.5 * Math.Min(Math.Pow(objectCount / 1500, 0.3), 1.15);
                    double fo0 = Math.Pow(AccValue, 1.1);
                    double fo1 = Math.Pow(StrainBase * strainMult, 1.1);
                    double final_output = Math.Round(Math.Pow(fo0 + fo1, Math.Round(1 / 1.1, 2)) * 1.1);
                    string output = Convert.ToString(final_output);
                    return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Log.Write($"Error: {ex}");
                return "Error.";
            }
        }

        public static string Combo(string map_id)
        {
            try
            {
                string max_combo;
                var client = new RestClient("https://osu.ppy.sh/api/");
                var request = new RestRequest($"get_scores?b={map_id}&k={Data.ApiKey}&m=3&limit=1");
                var response = client.Execute(request);
                string result = response.Content;
                if (result.Length > 2)
                {
                    var scr = JsonConvert.DeserializeObject<Scores>(result.Substring(1, result.Length - 2));
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

}
