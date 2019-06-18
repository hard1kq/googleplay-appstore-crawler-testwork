using System.Collections.Generic;
using System.Threading.Tasks;
using StoreParsers.Domain;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace StoreParsers.Infrastructure
{
    public class GooglePlayClient : IGooglePlayClient
    {
        private readonly HttpClient _client;
        private const string ScriptRegex = @">AF_initDataCallback[\s\S]*?<\/script";
        private const string KeyRegex = @"(ds:.*?)'";
        private const string ValueRegex = @"return ([\s\S]*?)}}\);<\/";

        public GooglePlayClient(HttpClient client)
        {
            _client = client;
        }
        
        public async Task<List<string>> GetTop3SearchSuggestAsync(string keyword)
        {
            var postData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("f.req", $@"[[[""lGYRle"",""[[null,[[10,[10,50]],true,null,[96,27,4,8,57,30,110,79,11,16,49,1,3,9,12,104,55,56,51,10,34,77]],[\""{keyword}\""],4,null,null,null,[]]]"",null,""vVuZ6e:238|Bc""]]]")
            });
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = postData,
                RequestUri = new Uri("https://play.google.com/_/PlayStoreUi/data/batchexecute?rpcids=lGYRle&f.sid=-3595495489652211591&bl=boq_playuiserver_20190423.04_p0&gl=ru&hl=ru&authuser=0&soc-app=121&soc-platform=1&soc-device=1&_reqid=451399&rt=c")
            };
            HttpResponseMessage response = await _client.SendAsync(message);
            string html = await response.Content.ReadAsStringAsync();
            Match jsonMatches = Regex.Match(html, @"""(\[.*\\n)""");
            string json = Regex.Unescape(jsonMatches.Groups[1].Value);
            return JArray.Parse(json).SelectTokens("[0][1][0][0][0][*][12][0]").Values<string>().Take(3).ToList();
        }

        public async Task<Application> GetApplicationInfo(string packageName)
        {
            var appPage = await GetApplicationPageAsync(packageName);
            var appJsonInfo = GetValidJson(appPage);

            return new Application
            {
                Name = (string)appJsonInfo.SelectToken("ds:5[0][0][0]"),
                Icon = (string)appJsonInfo.SelectToken("ds:5[0][12][1][3][2]"),
                Rating = (double)appJsonInfo.SelectToken("ds:7[0][6][0][1]"),
                Screenshots = appJsonInfo.SelectTokens("ds:5[0][12][0][*][3][2]")
                    .Values<string>()
                    .Select(s => new Screenshot { Url = s })
                    .ToList()
            };
        }

        private async Task<string> GetApplicationPageAsync(string packageName)
        {
            return await _client.GetStringAsync($"https://play.google.com/store/apps/details?id={packageName}&hl=ru&gl=ru");
        }

        private JObject GetValidJson(string html)
        {
            var appJsonInfo = new JObject();
            MatchCollection scripts = Regex.Matches(html, ScriptRegex);
            foreach (Match script in scripts)
            {
                Match keyMatch = Regex.Match(script.Value, KeyRegex);
                Match valueMatch = Regex.Match(script.Value, ValueRegex);
                if (keyMatch.Success && valueMatch.Success)
                {
                    string key = keyMatch.Groups[1].Value;
                    appJsonInfo[key] = JArray.Parse(valueMatch.Groups[1].Value);
                }
            }
            return appJsonInfo;
        }
    }
}
