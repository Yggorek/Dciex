using Dciex.Core.Projects;

namespace Dciex.Application.Projects;

public interface IProjectStatusChecker
{
    ProjectStatus Check(Project project);
}