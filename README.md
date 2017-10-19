# Cake.Deploy.Bot.LUIS ![Build status](https://ci.appveyor.com/api/projects/status/github/ObjectivityLtd/Cake.Deploy.Bot.LUIS?svg=true)
Cake addin for publishing a LUIS app.

www.luis.ai

## How to add Cake.Deploy.Bot.LUIS
In order to use this package add the following line in your addin section:
```cake
#addin "Cake.Deploy.Bot.LUIS"
```

In order to use specific version of addin:

```cake
#addin nuget:?package=Cake.Deploy.Bot.LUIS&version=0.0.1
```

This package has a dependency on a Newtosoft.Json >= 10.0.3 package, that must be added manually to Cake script or loaded by Cake dependency resolving mechanism. To use Cake dependency loading:

* Add folowinng lines to cake.config
```
[Nuget]
UseInProcessClient=true
LoadDependencies=true
```

or to load dependencies for selected packages only, use following sections

* cake.config
```
[Nuget]
UseInProcessClient=true
```

* addin section in Cake script

```cake
#addin nuget:?package=Cake.Deploy.Bot.LUIS&loaddependencies=true
```
## How to use Cake.Deploy.Bot.LUIS


```cake
#addin nuget:?package=Cake.Deploy.Bot.LUIS&version=0.0.1


Task("Publish LUIS app")
    .Does(()=>{
        var subscriptionKey = "123-abc-321";
        var appName = "exampleApp";
        var appVersion = "0.1";
        var region = "westus";
        var pathToJsonApp = "examplePath\\app.json"

        var endpoint = DeployLuisApp(subscriptionKey, appName, appVersion, region, pathToJsonApp);
    });
```