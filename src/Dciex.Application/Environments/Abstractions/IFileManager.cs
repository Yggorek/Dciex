using System;
using System.Collections.Generic;
using System.Text;

namespace Dciex.Application.Environments.Abstractions
{
    public interface IFileManager
    {
        Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);
        Task WriteAllTextAtomicAsync(string path, string content, CancellationToken cancellationToken = default);
    }
}
