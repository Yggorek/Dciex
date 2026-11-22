using System;
using System.Collections.Generic;
using System.Text;
using Dciex.Core.Projects;
namespace Dciex.Application.Projects
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
        Task<bool> UnregisterAsync(
             string name,
             CancellationToken cancellationToken = default);
    }
}
