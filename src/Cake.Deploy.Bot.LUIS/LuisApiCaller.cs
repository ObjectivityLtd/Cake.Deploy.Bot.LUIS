using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Resources;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cake.Deploy.Bot.LUIS
{
    public class LuisApiCaller
    {
        private readonly LuisApiUrlProvider _apiUrlProvider;

        private readonly LuisHttpClient _httpClient;

        public LuisApiCaller(LuisApiUrlProvider apiUrlProvider, LuisHttpClient httpClient)
        {
            _apiUrlProvider = apiUrlProvider;
            _httpClient = httpClient;
        }

        public IEnumerable<Version> GetAppVersions(string appId)
        {
            appId = appId ?? throw new ArgumentNullException(nameof(appId));

            var url = this._apiUrlProvider.GetGetApplicationVersionsUrl(appId);

            var response = this._httpClient.Get(url);

            var apps = response.Content.ReadAsStringAsync().Result;
            var array = JArray.Parse(apps);

            var versions = array.Select(q => new Version(q.Value<JObject>().Property("version").Value.ToString())).AsEnumerable();

            return versions;
        }

        public IDictionary<string, string> GetApps()
        {
            var url = this._apiUrlProvider.GetGetUserApplicationsUrl();

            var response = this._httpClient.Get(url);

            var apps = response.Content.ReadAsStringAsync().Result;
            var array = JArray.Parse(apps);

            var versions = array.Select(q => new {Name = q.Value<JObject>().Property("name").Value.ToString(), Id = q.Value<JObject>().Property("id").Value.ToString() }).ToDictionary(q => q.Name, p => p.Id);

            return versions;
        }

        public string ImportApplication(string appName, JObject luisApplication)
        {
            var url = this._apiUrlProvider.GetImportApplicationUrl(appName);

            var response = this._httpClient.PostJObject(url, luisApplication, CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }

            var result = response.Content.ReadAsStringAsync().Result;

            return result.Trim('\"');
        }

        public void ImportVersionToApplication(string appId, string versionNumber, JObject luisApplication)
        {
            var url = this._apiUrlProvider.GetImportVersionToApplicationUrl(appId, versionNumber);

            var response = this._httpClient.PostJObject(url, luisApplication, CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        public void AddSubscriptionKey(string appId, string keyPath, string subscriptionId)
        {
            var url = this._apiUrlProvider.GetAddSubscriptionKeyUrl(appId);

            var model = new {subscriptionId = subscriptionId, region = "westeurope", keyPath = keyPath};

            var byteData = JsonConvert.SerializeObject(model);
            using (var content = new StringContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = this._httpClient.Client.PostAsync(url, content, CancellationToken.None).Result;
            }
        }

        public Dictionary<string,string> GetAppEndpoints(string appId)
        {
            var url = this._apiUrlProvider.GetGetEndpointsUrl(appId);

            var response = this._httpClient.Get(url);

            var endpoints = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            var result = new Dictionary<string,string>();

            foreach (var keyValuePair in endpoints)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value.ToString());
            }

            return result;
        }

        public Version GetAppMaxVersion(string appId)
        {
            appId = appId ?? throw new ArgumentNullException(nameof(appId));

            var versions = this.GetAppVersions(appId);

            return versions.Max();
        }

        public string GetAppId(string appName)
        {
            appName = appName ?? throw new ArgumentNullException(nameof(appName));

            var apps = this.GetApps();

            var id = apps.ContainsKey(appName) ? apps[appName] : null;

            return id;
        }

        public void TrainAppVersion(string appId, Version version)
        {
            var url = this._apiUrlProvider.GetTrainApplicationVersionUrl(appId, version.ToString());

            var emptyObject = new JObject();

            var response = this._httpClient.PostJObject(url, emptyObject, CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        public Tuple<TrainingStatus, string> GetVersionTrainingStatus(string appId, Version version)
        {
            var url = this._apiUrlProvider.GetTrainApplicationVersionUrl(appId, version.ToString());

            var response = this._httpClient.Get(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }

            var a = JArray.Parse(response.Content.ReadAsStringAsync().Result);

            foreach (dynamic model in a)
            {
                var status = model.details.status.ToString();

                switch (status)
                {
                    case "Fail":
                        return new Tuple<TrainingStatus, string>(TrainingStatus.Fail, model.details.failureReason);
                    case "InProgress":
                        return new Tuple<TrainingStatus, string>(TrainingStatus.InProgress, "");
                    case "Queued":
                        return new Tuple<TrainingStatus, string>(TrainingStatus.Queued, "");
                }
            }

            return new Tuple<TrainingStatus,string>(TrainingStatus.Success, "");
        }

        public enum TrainingStatus
        {
            Fail,
            InProgress,
            Queued,
            Success
        }

        public void PublishAppVersion(string appId, Version version)
        {
            var url = this._apiUrlProvider.GetPublishApplicationUrl(appId);

            var model = new { isStaging = false, versionId = version.ToString(), region = string.Empty };
            var jObject = JObject.FromObject(model);

            var response = this._httpClient.PostJObject(url, jObject, CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }
        }
    }
}
