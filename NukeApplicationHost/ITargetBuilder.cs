namespace NukeApplicationHost;

public interface ITargetBuilder : INukePipelineBuilder
{
    Task Invoke(CancellationToken cancellationToken);
    IEnumerable<ITargetBuilder> BuildOrderedDependencies();
    ITargetBuilder DependsOn(ITargetBuilder target);
    string Name { get; }
}