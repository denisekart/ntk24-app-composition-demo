using Microsoft.Extensions.Logging;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace NukeApplicationHost.Tools;

public class DotnetBuild(ILogger<DotnetBuild> logger)
{
    public void Invoke(Configure<DotNetBuildSettings> configurator)
    {
        DotNetTasks.DotNetBuild(c => configurator.Invoke(c)
            .SetProcessLogger((type, text) =>
            {
                switch (type)
                {
                    case OutputType.Std:
                        logger.LogDebug("{Text}", text);
                        break;
                    case OutputType.Err:
                        logger.LogError("{Text}", text);
                        break;
                }
            }));
    }
}