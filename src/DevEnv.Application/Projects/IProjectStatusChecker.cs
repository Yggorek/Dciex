using DevEnv.Core.Projects;

namespace DevEnv.Application.Projects;

public interface IProjectStatusChecker
{
    ProjectStatus Check(Project project);
}