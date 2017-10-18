using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.Annotations;
using Newtonsoft.Json.Linq;

namespace Cake.Deploy.Bot.LUIS
{
    public static class CakeLuisExtension
    {
        [CakeMethodAlias]
        public static string DeployLuisApp(this ICakeContext context, string subscriptionKey, string appName, string version, string region, string pathToModel)
        {
            JObject model = JObject.Parse(File.ReadAllText(pathToModel));
            var luisManager = new LuisManager(subscriptionKey);

            luisManager.DeleteAppByNameAsync(appName, CancellationToken.None).Wait();
            var appId = luisManager.ImportAppAsync(model, appName, CancellationToken.None).Result;
            luisManager.TrainAppByIdAndVersionAsync(appId, version, CancellationToken.None).Wait();
            luisManager.PublishAppVersion(appId, version, CancellationToken.None).Wait();
            var endpoint = luisManager.GetAppEndpoints(appId, region, CancellationToken.None).Result;

            return endpoint;
        }
    }
}

