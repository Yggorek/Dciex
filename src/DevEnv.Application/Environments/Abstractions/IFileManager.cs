using System;
using System.Collections.Generic;
using System.Text;

namespace DevEnv.Application.Environments.Abstractions
{
    public interface IFileManager
    {
        Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);
    }
}
