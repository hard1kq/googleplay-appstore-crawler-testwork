using Newtonsoft.Json.Linq;
using StoreParsers.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StoreParsers.Infrastructure
{
    public class AppStoreClient : IAppStoreClient
    {
        private readonly HttpClient _client;

        public AppStoreClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<Application> GetApplicationInfo(string uniqueId)
        {
            string response = await _client.GetStringAsync($"https://itunes.apple.com/ru/app/id{uniqueId}?mt=8&isWebExpV2=true&dataOnly=true");
            var appData = JObject.Parse(response);
            return new Application
            {
                Icon = (string)appData.SelectToken($"$.storePlatformData.webexp-product.results.{uniqueId}.artwork.url"),
                Name = (string)appData.SelectToken($"$.storePlatformData.webexp-product.results.{uniqueId}.name"),
                Rating = (double)appData.SelectToken($"$.storePlatformData.webexp-product.results.{uniqueId}.userRating.value"),
                Screenshots = appData.SelectTokens($"$.storePlatformData.webexp-product.results.{uniqueId}.screenshotsByType.*[*].url")
                    .Values<string>()
                    .Select(s => new Screenshot { Url = s })
                    .ToList()
            };
        }

        public async Task<List<string>> GetTop3SearchSuggestAsync(string keyword)
        {
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://itunes.apple.com/WebObjects/MZSearch.woa/wa/search?clientApplication=Software&term={keyword}"),
                Headers = { { "X-Apple-Store-Front", "143469,24 t:native" } }
            };

            HttpResponseMessage response = await _client.SendAsync(message);
            string result = await response.Content.ReadAsStringAsync();
            return JObject.Parse(result).SelectTokens("$.bubbles[*].results[:3].id").Values<string>().ToList();
        }
    }
}
