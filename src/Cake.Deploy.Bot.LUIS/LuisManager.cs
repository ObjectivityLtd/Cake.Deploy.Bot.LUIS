using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cake.Deploy.Bot.LUIS
{
    public class LuisManager
    {
        private readonly HttpClient _httpClient;

        private readonly string _baseUrl;

        private readonly string _region;

        public LuisManager(string subscriptionKey, string region)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            _baseUrl = LuisUrlHelper.GetApiUrl(region);
            _region = region;
        }

        public async Task<string> ImportAppAsync(JObject model, string appName, CancellationToken ct)
        {
            var uri = $"{_baseUrl}/apps/import?appName={appName}";

            HttpResponseMessage response;
            var byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await _httpClient.PostAsync(uri, content, ct);
            }
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }

            var id = await response.Content.ReadAsStringAsync();
            return id.Replace("\"", "");
        }

        public async Task<bool> DeleteAppByIdAsync(string appId, CancellationToken ct)
        {
            var uri = $"{_baseUrl}/apps/{appId}";
            var response = await _httpClient.DeleteAsync(uri, ct);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAppByNameAsync(string appName, CancellationToken ct)
        {

            var appId = await this.GetAppIdByNameAsync(appName, ct);

            return this.DeleteAppByIdAsync(appId, ct).Result;
        }

        public async Task<bool> TrainAppByNameAndVersionAsync(string appName, string versionId, CancellationToken ct)
        {
            var appId = await this.GetAppIdByNameAsync(appName, ct);

            return this.TrainAppByIdAndVersionAsync(appId, versionId, ct).Result;
        }

        public async Task<bool> TrainAppByIdAndVersionAsync(string appId, string versionId, CancellationToken ct)
        {
            var uri = $"{_baseUrl}/apps/{appId}/versions/{versionId}/train";
            HttpResponseMessage response;
            var byteData = Encoding.UTF8.GetBytes(string.Empty);
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await _httpClient.PostAsync(uri, content, ct);
            }
            if (response.IsSuccessStatusCode)
            {
                bool isTrained = false;

                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                    var a = JArray.Parse(await (await _httpClient.GetAsync(uri, ct)).Content.ReadAsStringAsync());
                    isTrained = true;
                    foreach (dynamic model in a)
                    {
                        var status = model.details.status.ToString();
                        if (status == "Fail")
                        {
                            throw new Exception(model.details.failureReason);
                        }
                        else if (status == "Success")
                        {
                            continue;
                        }
                        else if (status == "InProgress")
                        {
                            isTrained = false;
                            break;
                        }
                    }
                } while (!isTrained);
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PublishAppVersion(string appId, string versionId, CancellationToken ct)
        {
            var uri = $"{_baseUrl}/apps/{appId}/publish";

            HttpResponseMessage response;
            var model = new { isStaging = false, versionId = versionId, region = string.Empty };
            var byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await _httpClient.PostAsync(uri, content, ct);
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<string> GetAppEndpoints(string appId, CancellationToken ct)
        {
            var uri = $"{_baseUrl}/apps/{appId}/endpoints";

            var endpoints = JObject.Parse(await (await _httpClient.GetAsync(uri, ct)).Content.ReadAsStringAsync());

            foreach (var endpoint in endpoints)
            {
                if (endpoint.Key == _region)
                {
                    return endpoint.Value.ToString();
                }
            }
            return string.Empty;
        }

        private async Task<string> GetAppIdByNameAsync(string appName, CancellationToken ct)
        {
            var uri = $"{_baseUrl}/apps/";
            var response = await _httpClient.GetAsync(uri, ct);
            var apps = await response.Content.ReadAsStringAsync();
            var array = JArray.Parse(apps);

            foreach (var app in array)
            {
                var appObject = app.Value<JObject>();
                var name = appObject.Property("name").Value.ToString();
                if (name == appName)
                {
                    return appObject.Property("id").Value.ToString();
                }
            }
            return string.Empty;
        }
    }
}