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

    }
}
