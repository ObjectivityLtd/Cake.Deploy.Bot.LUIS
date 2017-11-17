using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cake.Deploy.Bot.LUIS
{
    using Newtonsoft.Json.Linq;

    public class LuisAppManager
    {
        private readonly string _appName;

        private readonly LuisApiCaller _apiCaller;

        public LuisAppManager(string appName, LuisApiCaller apiCaller)
        {
            _appName = appName;
            _apiCaller = apiCaller;
        }

        public Tuple<string, Version> ImportApp(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            var jobject = JObject.Parse(File.ReadAllText(filePath));

            var id = _apiCaller.GetAppId(this._appName);

            Version appVersion = null;

            if (string.IsNullOrEmpty(id))
            {
                id = _apiCaller.ImportApplication(this._appName, jobject);

                appVersion = this._apiCaller.GetAppVersions(id).First();

            }
            else
            {
                var maxVersion = _apiCaller.GetAppMaxVersion(id);

                var newVersion = maxVersion.Add(new Version(0, 1));

                _apiCaller.ImportVersionToApplication(id, newVersion.ToString(), jobject);

                appVersion = newVersion;
            }

            this._apiCaller.TrainAppVersion(id, appVersion);

            this.WaitForTrained(id, appVersion);

            return new Tuple<string, Version>(id, appVersion);
        }

        public void AddKey(string azureSubscriptionId, string rgName, string cognitiveServiceName, string cognitiveServiceInternalId)
        {
            if (string.IsNullOrEmpty(azureSubscriptionId))
            {
                throw new ArgumentNullException(nameof(azureSubscriptionId));
            }

            if (string.IsNullOrEmpty(rgName))
            {
                throw new ArgumentNullException(nameof(rgName));
            }

            if (string.IsNullOrEmpty(cognitiveServiceName))
            {
                throw new ArgumentNullException(nameof(cognitiveServiceName));
            }

            if (string.IsNullOrEmpty(cognitiveServiceInternalId))
            {
                throw new ArgumentNullException(nameof(cognitiveServiceInternalId));
            }

            var id = _apiCaller.GetAppId(this._appName);

            var keyPath =
                $"/subscriptions/{azureSubscriptionId}/resourceGroups/{rgName}/providers/Microsoft.CognitiveServices/accounts/{cognitiveServiceName}";

            this._apiCaller.AddSubscriptionKey(id, keyPath, cognitiveServiceInternalId);
        }

        public string GetAppEndpoint(string id, Region region)
        {
            var endpoint = this._apiCaller.GetAppEndpoints(id);

            return endpoint.ContainsKey(region.ToString().ToLower()) ? endpoint[region.ToString().ToLower()] : null;
        }

        public void WaitForTrained(string appId, Version appVersion)
        {
            LuisApiCaller.TrainingStatus status = LuisApiCaller.TrainingStatus.InProgress;

            Tuple<LuisApiCaller.TrainingStatus, string> statusResult = null;

            while (status != LuisApiCaller.TrainingStatus.Fail && status != LuisApiCaller.TrainingStatus.Success)
            {
                statusResult = this._apiCaller.GetVersionTrainingStatus(appId, appVersion);
                status = statusResult.Item1;

                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }

            if (status == LuisApiCaller.TrainingStatus.Fail)
            {
                throw new Exception(statusResult.Item2);
            }
        }
    }
}
