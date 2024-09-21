using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using NukeApplicationHost;

var builder = NukeApplication.CreateBuilder(args);
builder.Services.AddKeyedScoped<Tool>("dotnet", (_,_) => DotNetTasks.DotNet);
builder.AddDefaultSolution("BuildAutomationHost.sln");

var app = builder.Build();

app.MapTarget("default", (ILogger<Program> logger, [FromKeyedServices("dotnet")] Tool dotnet) => { });

app.Run();
