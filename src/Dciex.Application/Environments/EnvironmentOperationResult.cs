using System;
using System.Collections.Generic;

namespace Dciex.Application.Environments
{
    public class EnvironmentOperationResult
    {
        public bool IsSuccess { get; init; }
        public EnvironmentErrorCode? ErrorCode { get; init; }
        public string? Message { get; init; }
        public IReadOnlyList<EnvironmentOperationError> Errors { get; init; } = Array.Empty<EnvironmentOperationError>();
    }

    public enum EnvironmentErrorCode
    { 
       ProjectNotFound,
       ProjectRegistryLoadFailed,
       ConfigurationLoadFailed,
       ComposeGenerationFailed,
       FileWriteFailed,
       DockerStartFailed,
       ComposeNotFound,
       ComposeAmbiguous,
       ProjectDirectoryNotFound,
       ComposeDiscoveryFailed
    }
}
