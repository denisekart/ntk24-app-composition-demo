using Nuke.Common.ProjectModel;

namespace NukeApplicationHost.Tools;

public class DefaultProject
{
    public void Set(Project? project) => Value = project;
    public Project? Value { get; private set; }
}