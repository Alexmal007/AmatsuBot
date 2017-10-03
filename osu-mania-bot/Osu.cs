using System;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;

namespace Amatsu
{
    class Osu
    {
        public static List<Double> GetAveragePP(string username)
        {
            try
            {
                Double pp = 0;
                Double acc = 0;
                RestClient client = new RestClient("https://osu.ppy.sh/api");
                RestRequest request = new RestRequest($"/get_user_best?u={username}&k={Data.ApiKey}&limit=10&m=3");
                client.Timeout = 5000;
                request.Timeout = 5000;
                IRestResponse response = client.Execute(request);
                string result = response.Content;
                if (result.Length > 2 && !result.Contains("error"))
                {
                    UserBest[] usb = JsonConvert.DeserializeObject<UserBest[]>(result);
                    for (int i = 0; i < usb.Length; i++)
                    {
                        Double acc0 = (Convert.ToDouble(usb[i].count300) * 100 + Convert.ToDouble(usb[i].count100) * 33.333 + Convert.ToDouble(usb[i].count50) * 16.666 + Convert.ToDouble(usb[i].countmiss) * 0) / (Convert.ToDouble(usb[i].count300) + Convert.ToDouble(usb[i].count100) + Convert.ToDouble(usb[i].count50) + Convert.ToDouble(usb[i].countmiss));
                        pp = pp + Convert.ToDouble(usb[i].pp.Replace('.', ','));
                        acc = acc + acc0;
                    }
                    pp = pp / usb.Length;
                    acc = acc / usb.Length;
                    var output = new List<Double>();
                    output.Add(pp);
                    output.Add(acc);
                    return output;
                }
                else
                {
                    var output = new List<Double>();
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
                return output;
            }
        }


        public static string Calculate(Double acc, Double obj, Double stars, Double od)
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
                    Double final_output = Math.Round(Math.Pow(fo0 + fo1, Math.Round(1 / 1.1, 2)) * 1.1);

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
                RestClient client = new RestClient("https://osu.ppy.sh/api/");
                RestRequest request = new RestRequest($"get_scores?b={map_id}&k={Data.ApiKey}&m=3&limit=1");
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

        public static string GetKeys(string username)
        {
            try
            {
                var keys4Count = 0;
                var keys7Count = 0;
                var client = new RestClient("https://osu.ppy.sh/api/");
                var request = new RestRequest($"get_user_best?u={username}&k={Data.ApiKey}&m=3&limit=100");
                client.Timeout = 5000;
                request.Timeout = 5000;
                var result = client.Execute(request).Content;
                if (result.Length > 2)
                {
                    var topScores = JsonConvert.DeserializeObject<UserBest[]>(result);
                    for (int i = 0; i < topScores.Length; i++)
                    {
                        var mapInfo = new MapInfo(topScores[i].beatmap_id);
                        if (mapInfo.keys == 4)
                        {
                            keys4Count++;
                        }
                        else if (mapInfo.keys == 7)
                        {
                            keys7Count++;
                        }
                    }
                    if (keys4Count >= keys7Count)
                    {
                        return $"{keys4Count}";
                    }
                    else if (keys4Count < keys7Count)
                    {
                        return $"{keys7Count}";
                    }
                    else
                    {
                        Console.WriteLine("Keys error.");
                        Log.Write($"Keys error. Response length: {result.Length}, keys4Count: {keys4Count}, keys7Count: {keys7Count}.");
                        return "Error occuried.";
                    }
                }
                else
                {
                    Console.WriteLine($"Error occuried. Result length: {result.Length}.");
                    Log.Write($"Error occuried. Result length: {result.Length}.");
                    return "Error occuried.";
                }
            }
            catch (Exception ex)
            {
                Log.Write($"{ex}");
                Console.WriteLine(ex);
                return "Error occuried.";
            }
        }

    }

}
