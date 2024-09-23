using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NukeApplicationHost;

public class NukeApplication(IHost host) : INukePipelineBuilder
{
    private readonly Dictionary<string, ITargetBuilder> _targets = new Dictionary<string, ITargetBuilder>(StringComparer.OrdinalIgnoreCase);
    public static NukeApplicationBuilder CreateBuilder(params string[] args) => new NukeApplicationBuilder(args);

    CancellationTokenSource? _cts;
    Task? _runner;

    public void Dispose()
    {
        host?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _runner = Task<Task?>.Factory.StartNew(() => RunApplication(_cts.Token), _cts.Token);
        return host.StartAsync(_cts.Token);
    }

    async Task RunApplication(CancellationToken stoppingToken)
    {
        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Nuke");
        var target = host.Services
                .GetRequiredService<IConfiguration>()
                .GetValue<string?>("Target")
            ?? throw new ArgumentException("Expected 'Target' to be set.");

        if (_targets.TryGetValue(target, out var targetBuilder))
        {
            var allTargets = targetBuilder.BuildOrderedDependencies().ToArray();
            foreach (var targetInstance in allTargets)
            {
                logger.LogInformation("Invoking target {Target}", targetInstance.Name);
                await targetInstance.Invoke(stoppingToken);
            }
        }

        logger.LogInformation("Done");
        host.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.WhenAll(host.StopAsync(cancellationToken), _cts!.CancelAsync());
    }

    public IServiceProvider Services => host.Services;

    public ITargetBuilder MapTarget(string targetName, Delegate action)
    {
        var builder = new NukeTargetBuilder(this, action, targetName);
        _targets.Add(targetName, builder);

        return builder;
    }

    public ITargetBuilder MapTarget(Delegate action)
    {
        var builder = new NukeTargetBuilder(this, action, "<anonymous>");

        return builder;
    }
}