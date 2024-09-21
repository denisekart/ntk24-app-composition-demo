using Microsoft.Extensions.Hosting;

namespace NukeApplicationHost;

public interface INukePipelineBuilder : IHost
{
    ITargetBuilder MapTarget(string targetName, Delegate action);
}