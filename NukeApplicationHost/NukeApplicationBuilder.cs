using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nuke.Common;
using NukeApplicationHost.Tools;

namespace NukeApplicationHost;

public class NukeApplicationBuilder : IHostApplicationBuilder
{
    internal NukeApplicationBuilder(params string[] args)
    {
        _builder = new HostApplicationBuilder(new HostApplicationBuilderSettings()
        {
            Args = args,
            ContentRootPath = Directory.GetCurrentDirectory(),
        });
        _builder.Configuration.AddCommandLine(args,
            new Dictionary<string, string>
            {
                { "-t", "target" }
            });
        _builder.Services.AddScoped<DefaultProject>();
        _builder.Services.AddScoped<DefaultSolution>();
    }

    readonly HostApplicationBuilder _builder;

    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull
    {
        _builder.ConfigureContainer(factory, configure);
    }

    public IDictionary<object, object> Properties => ((IHostApplicationBuilder)_builder).Properties;

    public IConfigurationManager Configuration => _builder.Configuration;

    public IHostEnvironment Environment => _builder.Environment;

    public ILoggingBuilder Logging => _builder.Logging;

    public IMetricsBuilder Metrics => _builder.Metrics;

    public IServiceCollection Services => _builder.Services;

    public NukeApplication Build() => new NukeApplication(_builder.Build());
}