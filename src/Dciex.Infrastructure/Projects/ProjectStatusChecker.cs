using Dciex.Application.Projects;
using Dciex.Core.Projects;

namespace Dciex.Infrastructure.Projects;
public class ProjectStatusChecker : IProjectStatusChecker
{
    public ProjectStatus Check(Project project)
    { 
        if (System.IO.Directory.Exists(project.Path))
        {
            return ProjectStatus.Available;
        }
        else
        {
            return ProjectStatus.Missing;
        }
    }
    
}