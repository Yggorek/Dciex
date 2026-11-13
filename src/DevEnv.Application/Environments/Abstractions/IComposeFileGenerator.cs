using DevEnv.Core.Configuration;
using DevEnv.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevEnv.Application.Environments.Abstractions
{
    public interface IComposeFileGenerator
    {
        string Generate(EnvironmentConfiguration configuration);
    }
}
