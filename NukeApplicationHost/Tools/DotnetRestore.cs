using Microsoft.Extensions.Logging;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace NukeApplicationHost.Tools;

public class DotnetRestore(ILogger<DotnetRestore> logger)
{
    public void Invoke(Configure<DotNetRestoreSettings> configurator)
    {
        DotNetTasks.DotNetRestore(c => configurator.Invoke(c)
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
