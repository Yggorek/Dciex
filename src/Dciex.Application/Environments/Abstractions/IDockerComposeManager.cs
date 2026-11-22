using System;
using System.Collections.Generic;
using System.Text;

namespace Dciex.Application.Environments.Abstractions
{
    public interface IDockerComposeManager
    {
        Task StartAsync(string workingDirectory, CancellationToken cancellationToken = default);
    }
}
