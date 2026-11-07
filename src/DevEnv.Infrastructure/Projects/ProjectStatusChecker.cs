using DevEnv.Application.Projects;
using DevEnv.Core.Projects;

namespace DevEnv.Infrastructure.Projects;
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