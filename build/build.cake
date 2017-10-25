
///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var sourceDir = "..\\src";
var outputDir = "..\\bin";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    Information("Running tasks...");

    if(!DirectoryExists(outputDir))
    {
        Information("Output directory does not exist.");
        CreateDirectory(outputDir);
    }
    else
    {
        CleanDirectory(outputDir);
    }
});

Teardown(() =>
{
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("BuildSolution")
    .Description("Builds Cake.Deploy.Bot.LUIS")
    .Does(() =>
{
    var solution = sourceDir + "\\Cake.Deploy.Bot.LUIS.sln";

    NuGetRestore(solution);

    var buildOutputDir = "\"" + MakeAbsolute(Directory(outputDir)).FullPath + "\"";
    Information(buildOutputDir);

    // var buildSettings = new MSBuildSettings()
    //     .SetConfiguration(configuration)
    //     .WithProperty("OutputPath", buildOutputDir);
        
    MSBuild(solution, settings => 
        settings.SetConfiguration(configuration));
});

Task("NuGet")
    .Description("Create nuget package")
    .Does(()=>
{
    var projectFile = sourceDir + "\\Cake.Deploy.Bot.LUIS\\Cake.Deploy.Bot.LUIS.csproj";

    var nuGetPackSettings   = new NuGetPackSettings {
        OutputDirectory = outputDir,
        Properties = new Dictionary<string,string>{ {"Configuration", configuration} }
    };

    NuGetPack(projectFile, nuGetPackSettings);
});

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .Description("This is the default task which will be ran if no specific target is passed in.")
    .IsDependentOn("BuildSolution")
    .IsDependentOn("NuGet");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);