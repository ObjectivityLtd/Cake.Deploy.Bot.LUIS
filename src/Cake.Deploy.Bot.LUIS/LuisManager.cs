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

        public LuisManager(string subscriptionKey)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
        }

        public async Task<string> ImportAppAsync(JObject model, string appName, CancellationToken ct)
        {
            var uri = $"https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/import?appName={appName}";

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
            var uri = $"https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/{appId}";
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
            var uri = $"https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/{appId}/versions/{versionId}/train";
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
                        var status = model.details.dtatusId;
                        if (status == 3)
                        {
                            throw new Exception(model.details.failureReason);
                        }
                        else if (status == 2)
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
            var uri = $"https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/{appId}/publish";

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

        public async Task<string> GetAppEndpoints(string appId, string region, CancellationToken ct)
        {
            var uri = $"https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/{appId}/endpoints";

            var endpoints = JObject.Parse(await (await _httpClient.GetAsync(uri, ct)).Content.ReadAsStringAsync());

            foreach (var endpoint in endpoints)
            {
                if (endpoint.Key == region)
                {
                    return endpoint.Value.ToString();
                }
            }
            return string.Empty;
        }

        private async Task<string> GetAppIdByNameAsync(string appName, CancellationToken ct)
        {
            var uri = $"https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/";
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