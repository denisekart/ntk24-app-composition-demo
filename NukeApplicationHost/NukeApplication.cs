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

public interface ITargetBuilder : INukePipelineBuilder
{
    Task Invoke(CancellationToken cancellationToken);
    IEnumerable<ITargetBuilder> BuildOrderedDependencies();
    ITargetBuilder DependsOn(ITargetBuilder target);
    string Name { get; }
}

public class NukeTargetBuilder(INukePipelineBuilder host, Delegate action, string name) : ITargetBuilder
{
    public string Name { get; } = name;
    readonly Delegate _action = action;

    readonly List<ITargetBuilder> _dependencies = new();

    public IEnumerable<ITargetBuilder> BuildOrderedDependencies()
    {
        HashSet<ITargetBuilder> visited = new(ReferenceEqualityComparer.Instance);

        foreach (var dependency in _dependencies)
        {
            var children = dependency.BuildOrderedDependencies();
            foreach (var child in children)
            {
                if (visited.Add(child))
                {
                    yield return child;
                }
            }

            if (visited.Add(dependency))
            {
                yield return dependency;
            }
        }

        yield return this;
    }

    public ITargetBuilder DependsOn(ITargetBuilder target)
    {
        _dependencies.Add(target);
        return this;
    }

    public async Task Invoke(CancellationToken cancellationToken)
    {
        var args = _action.Method
            .GetParameters()
            .Select(x => x.ParameterType)
            .Select(x => x switch
            {
                _ when x == typeof(CancellationToken) => cancellationToken,
                _ => ActivatorUtilities.GetServiceOrCreateInstance(host.Services, x)
            })
            .ToArray();

        var result = _action.Method.Invoke(_action.Target, args);
        if (result is Task task)
        {
            await task;
        }
    }

    public void Dispose() => host.Dispose();

    public async Task StartAsync(CancellationToken cancellationToken = new CancellationToken()) => await host.StartAsync(cancellationToken);

    public async Task StopAsync(CancellationToken cancellationToken = new CancellationToken()) => await host.StopAsync(cancellationToken);

    public IServiceProvider Services => host.Services;
    public ITargetBuilder MapTarget(string targetName, Delegate action) => host.MapTarget(targetName, action);
    public ITargetBuilder MapTarget(Delegate action) => host.MapTarget(action);
}
