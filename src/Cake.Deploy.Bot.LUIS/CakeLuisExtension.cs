using System.IO;
using System.Threading;
using Cake.Core;
using Cake.Core.Annotations;
using Newtonsoft.Json.Linq;

namespace Cake.Deploy.Bot.LUIS
{
    public static class CakeLuisExtension
    {
        [CakeMethodAlias]
        public static string DeployLuisApp(this ICakeContext context, string subscriptionKey, string appName, string version, string pathToModel, string region = "westeurope")
        {
            JObject model = JObject.Parse(File.ReadAllText(pathToModel));
            var luisManager = new LuisManager(subscriptionKey, region);

            luisManager.DeleteAppByNameAsync(appName, CancellationToken.None).Wait();
            var appId = luisManager.ImportAppAsync(model, appName, CancellationToken.None).Result;
            luisManager.TrainAppByIdAndVersionAsync(appId, version, CancellationToken.None).Wait();
            luisManager.PublishAppVersion(appId, version, CancellationToken.None).Wait();
            var endpoint = luisManager.GetAppEndpoints(appId, CancellationToken.None).Result;

            return endpoint;
        }
    }
}

