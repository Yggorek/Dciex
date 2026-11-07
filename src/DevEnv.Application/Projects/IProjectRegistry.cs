using System;
using System.Collections.Generic;
using System.Text;
using DevEnv.Core.Projects;
namespace DevEnv.Application.Projects
{
    public interface IProjectRegistry
    {
        Task<IReadOnlyList<Project>> GetAllAsync (
            CancellationToken cancellationToken = default);
        
        Task<Project?> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default);
        Task<Project> RegisterAsync(
             string path,
             CancellationToken cancellationToken = default);
    }
}
