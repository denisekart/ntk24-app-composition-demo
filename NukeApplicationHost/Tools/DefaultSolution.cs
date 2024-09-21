using Nuke.Common.ProjectModel;

namespace NukeApplicationHost.Tools;

public class DefaultSolution
{
    public void Set(Solution? solution) => Value = solution;
    public Solution? Value { get; private set; }
}
