using System;
using System.Collections.Generic;
using System.Text;

namespace DevEnv.Application.Environments
{
    public class EnvironmentOperationResult
    {
        public bool IsSuccess { get; init; }
        public EnvironmentErrorCode? ErrorCode { get; init; }
        public string? Message { get; init; }
    }

    public enum EnvironmentErrorCode
    { 
       ProjectNotFound,
       ConfigurationLoadFailed,
       ComposeGenerationFailed,
       FileWriteFailed,
       DockerStartFailed
    }
}
