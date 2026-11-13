using System;
using System.Collections.Generic;
using System.Text;

namespace DevEnv.Application.Environments.Abstractions
{
    public interface IDockerComposeManager
    {
        Task StartAsync(string workingDirectory, CancellationToken cancellationToken = default);
    }
}
