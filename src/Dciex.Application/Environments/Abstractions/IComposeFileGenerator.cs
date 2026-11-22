using Dciex.Core.Configuration;
using Dciex.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dciex.Application.Environments.Abstractions
{
    public interface IComposeFileGenerator
    {
        string Generate(EnvironmentConfiguration configuration);
    }
}
