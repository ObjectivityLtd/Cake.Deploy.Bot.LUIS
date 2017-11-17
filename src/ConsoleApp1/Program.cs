namespace ConsoleApp1
{
    using System.Threading;
    using Newtonsoft.Json.Linq;

    using System.IO;

    using Cake.Deploy.Bot.LUIS;



    class Program
    {
        static void Main(string[] args)
        {
            var subscriptionKey = "a25033a167464b5e862b921c316285c5";
            var region = "westeurope";
            var appName = "sampleLuisApp";
            var description = "app Description";
            var culture = "en-us";
            var usageScenario = "IoT";
            var domain = "Comics";
            var azureSubscriptionId = "971327b3-02d6-4219-bb68-3fecaa9eaadf";
            var resourceGroupName = "rg-sampleBot-dev";
            var cognitiveServiceName = "luisfortestsamplebot";
            var cognitiveServiceInternalId = "a060b66503e84fefa88e9bdc998e65f4";


            //var luisManager = new LuisManager(subscriptionKey, region);
            //var id = luisManager.GetAppIdByNameAsync(appName, CancellationToken.None).Result;

            //if (string.IsNullOrEmpty(id))
            //{
            //id = luisManager.CreateAppAsync(appName, description, culture, usageScenario, domain,
            //  CancellationToken.None).Result.Trim('\"');
            //}
            //var model = JObject.Parse(File.ReadAllText("C:\\Workspace\\Repo\\GitHub ObjLtd\\Cake.Deploy.Bot.LUIS\\src\\ConsoleApp1\\bin\\Debug\\luis\\sample-luis.json"));
            //id = luisManager.ImportAppVersionAsync(model, id, CancellationToken.None).Result;

            //var version = luisManager.GetAppVersions(id, CancellationToken.None).Result;

            //id = luisManager.ImportAppVersionAsync(model, id, version, CancellationToken.None).Result;

            //ICakeContext context = null;

            //context.DeployLuisApp("a25033a167464b5e862b921c316285c5", "test", "0.1",
            //  "C:\\Workspace\\Repo\\GitHub ObjLtd\\Cake.Deploy.Bot.LUIS\\src\\ConsoleApp1\\bin\\Debug\\luis\\sample-luis.json");


            var urlProvider = new LuisApiUrlProvider(Region.WestEurope);
            var client = new LuisHttpClient(subscriptionKey);
            var caller = new LuisApiCaller(urlProvider, client);

            var manager = new LuisAppManager(appName, caller);

            //var version = manager.ImportAppVersion("C:\\Workspace\\Repo\\GitHub ObjLtd\\Cake.Deploy.Bot.LUIS\\src\\ConsoleApp1\\bin\\Debug\\luis\\sample-luis.json");

            //manager.ImportApp("C:\\Workspace\\Repo\\Git Sandbox\\SampleBot\\luis\\sample-luis.json");


            var rdetail = CakeLuisExtension.DeployLuisApp(null, subscriptionKey, appName, Region.WestEurope,
                "C:\\Workspace\\Repo\\Git Sandbox\\SampleBot\\luis\\sample-luis.json");
        }
    }
}
