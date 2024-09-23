using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using NukeApplicationHost.Tools;

namespace NukeApplicationHost;

public static class NukeApplicationBuilderExtensions
{
    public static T SetDefaultSolution<T>(this T builder, string solutionPath) where T : IHostApplicationBuilder
    {
        var basePath = (AbsolutePath)builder.Environment.ContentRootPath;
        while (!(basePath / solutionPath).FileExists() && basePath.Parent.DirectoryExists())
        {
            basePath = basePath.Parent;
        }

        var eagerlyLoadedSolution = (basePath / solutionPath).ReadSolution();
        builder.Services.Replace(ServiceDescriptor.Scoped<DefaultSolution>(_ =>
        {
            var sln = new DefaultSolution();
            sln.Set(eagerlyLoadedSolution);

            return sln;
        }));

        return builder;
    }
}