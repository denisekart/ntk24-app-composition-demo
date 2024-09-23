using Microsoft.Extensions.DependencyInjection;

namespace NukeApplicationHost;

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