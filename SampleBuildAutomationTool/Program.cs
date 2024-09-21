using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using NukeApplicationHost;
using NukeApplicationHost.Tools;

var builder = NukeApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(opts => opts.SingleLine = true);
builder.SetDefaultSolution("BuildAutomationHost.sln");
builder.Services.AddScoped<DotnetBuild>();
builder.Services.AddScoped<DotnetRestore>();
builder.Services.AddScoped<DotnetClean>();
builder.Services.AddScoped<DotnetRun>();

var app = builder.Build();

var defaultProject = app.MapTarget((DefaultSolution solution, DefaultProject currentProject)
    => currentProject.Set(solution.Value!.GetProject("0_Welcome")));

var clean = app.MapTarget("clean",
        (DotnetClean clean, DefaultProject project)
            => clean.Invoke(c => c.SetProject(project.Value)))
    .DependsOn(defaultProject);

var restore = app.MapTarget("restore",
        (DotnetRestore restore, DefaultProject project)
            => restore.Invoke(c => c.SetProjectFile(project.Value)))
    .DependsOn(clean)
    .DependsOn(defaultProject);

var compile = app.MapTarget("compile",
        (DotnetBuild build, DefaultProject project)
            => build.Invoke(c => c.SetProjectFile(project.Value)))
    .DependsOn(restore)
    .DependsOn(defaultProject);

app.MapTarget("run",
        (DotnetRun run, DefaultProject project)
            => run.Invoke(c => c.SetProjectFile(project.Value)))
    .DependsOn(defaultProject);

app.Run();
