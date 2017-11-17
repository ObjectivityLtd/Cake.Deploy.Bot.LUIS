using System;
using System.IO;
using System.Threading;
using Cake.Core;
using Cake.Core.Annotations;
using Newtonsoft.Json.Linq;

namespace Cake.Deploy.Bot.LUIS
{
    public static class CakeLuisExtension
    {
        //[CakeMethodAlias]
        //[CakeNamespaceImport("Cake.Deploy.Bot.LUIS")]
        //public static LuisDetails DeployLuisApp(this ICakeContext context, string subscriptionKey, string appName, string version, string pathToModel, string region = "westeurope")
        //{
        //    JObject model = JObject.Parse(File.ReadAllText(pathToModel));
        //    var luisManager = new LuisManager(subscriptionKey, region);

        //    luisManager.DeleteAppByNameAsync(appName, CancellationToken.None).Wait();
        //    var appId = luisManager.ImportAppAsync(model, appName, CancellationToken.None).Result;
        //    luisManager.TrainAppByIdAndVersionAsync(appId, version, CancellationToken.None).Wait();
        //    luisManager.PublishAppVersion(appId, version, CancellationToken.None).Wait();
        //    var endpoint = luisManager.GetAppEndpoints(appId, CancellationToken.None).Result;

        //    var luisDetails = new LuisDetails
        //    {
        //        AppId = appId,
        //        Domain = new Uri(endpoint).Host
        //    };
        //    return luisDetails;
        //}

        [CakeMethodAlias]
        [CakeNamespaceImport("Cake.Deploy.Bot.LUIS")]
        public static LuisDetails DeployLuisApp(this ICakeContext context, string luisProgrammaticKey, string appName, Region region, string pathToModel)
        {

            var urlProvider = new LuisApiUrlProvider(region);
            var client = new LuisHttpClient(luisProgrammaticKey);
            var caller = new LuisApiCaller(urlProvider, client);
            var manager = new LuisAppManager(appName, caller);

            var app = manager.ImportApp(pathToModel);

            var domain = manager.GetAppEndpoint(app.Item1, region);

            return new LuisDetails() {AppId = app.Item1, Version = app.Item2, Domain = domain};
        }

        [CakeMethodAlias]
        [CakeNamespaceImport("Cake.Deploy.Bot.LUIS")]
        public static void AddAzureCognitiveServiceKey(this ICakeContext context, string luisProgrammaticKey, string appName, Region region, string azureSubscriptionId, string resourceGroupName, string cognitiveServiceName, string cognitiveServiceInternalId)
        {

            var urlProvider = new LuisApiUrlProvider(region);
            var client = new LuisHttpClient(luisProgrammaticKey);
            var caller = new LuisApiCaller(urlProvider, client);
            var manager = new LuisAppManager(appName, caller);

            manager.AddKey(azureSubscriptionId, resourceGroupName, cognitiveServiceName, cognitiveServiceInternalId);
        }
    }
}

