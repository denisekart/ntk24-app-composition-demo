using Microsoft.Extensions.Logging;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace NukeApplicationHost.Tools;

public class DotnetRun(ILogger<DotnetRun> logger)
{
    public void Invoke(Configure<DotNetRunSettings> configurator)
    {
        DotNetTasks.DotNetRun(c => configurator.Invoke(c)
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
