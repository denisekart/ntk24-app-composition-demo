using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NukeApplicationHost;

public class NukeApplication(IHost host) : INukePipelineBuilder
{
    private readonly Dictionary<string, ITargetBuilder> _targets = [];
    public static NukeApplicationBuilder CreateBuilder(params string[] args) => new NukeApplicationBuilder(args);

    public void Dispose()
    {
        host?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return host.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return host.StopAsync(cancellationToken);
    }

    public IServiceProvider Services => host.Services;

    public ITargetBuilder MapTarget(string targetName, Delegate action)
    {
        var builder = new NukeTargetBuilder(this, action);
        _targets.Add(targetName, builder);

        return builder;
        // var args = action.Method
        //     .GetParameters()
        //     .Select(x => x.ParameterType);
        //
        // var activator = ActivatorUtilities.(action.Target.GetType(), args.ToArray());
        // action.Method.Invoke(action.Target, activator.)
        // activator.Invoke(host.Services, []);
        // throw null;
    }
}

public interface ITargetBuilder : INukePipelineBuilder
{
}

public class NukeTargetBuilder(INukePipelineBuilder host, Delegate action) : ITargetBuilder
{
    public void Dispose() => host.Dispose();

    public async Task StartAsync(CancellationToken cancellationToken = new CancellationToken()) => await host.StartAsync(cancellationToken);

    public async Task StopAsync(CancellationToken cancellationToken = new CancellationToken()) => await host.StopAsync(cancellationToken);

    public IServiceProvider Services => host.Services;
    public ITargetBuilder MapTarget(string targetName, Delegate action) => host.MapTarget(targetName, action);
}
