using System;

namespace Cake.Deploy.Bot.LUIS
{
    public partial class LuisApiUrlProvider
    {
        private readonly string _baseApiUrl;
        private readonly string _baseWebApiUrl;

        public LuisApiUrlProvider(Region service)
        {
            switch (service)
            {
                case Region.WestEurope:
                    this._baseApiUrl = "https://westeurope.api.cognitive.microsoft.com/luis/api/v3.0";
                    this._baseWebApiUrl = "https://westeurope.api.cognitive.microsoft.com/luis/webapi/v3.0";
                    break;
                case Region.WestUs:
                    this._baseApiUrl = "https://westus.api.cognitive.microsoft.com/luis/api/v3.0";
                    this._baseWebApiUrl = "https://westus.api.cognitive.microsoft.com/luis/webapi/v3.0";
                    break;
            }
        }

        public string GetImportVersionToApplicationUrl(string appId, string versionNumber)
        {
            return $"{this._baseApiUrl}/apps/{appId}/versions/import?versionId={versionNumber}";
        }

        public string GetGetApplicationVersionsUrl(string appId)
        {
            return $"{this._baseApiUrl}/apps/{appId}/versions";
        }

        public string GetGetUserApplicationsUrl()
        {
            return $"{this._baseApiUrl}/apps/";
        }

        public string GetAddSubscriptionKeyUrl(string appId)
        {
            return $"{this._baseWebApiUrl}/apps/{appId}/subscriptions";
        }

        public string GetImportApplicationUrl(string appName)
        {
            return $"{this._baseApiUrl}/apps/import?appName={appName}";
        }

        public string GetGetEndpointsUrl(string appId)
        {
            return $"{this._baseApiUrl}/apps/{appId}/endpoints";
        }

        public string GetTrainApplicationVersionUrl(string appId, string version)
        {
            return $"{this._baseApiUrl}/apps/{appId}/versions/{version}/train";
        }

        public string GetPublishApplicationUrl(string appId)
        {
            return $"{this._baseApiUrl}/apps/{appId}/publish";
        }
    }
}
