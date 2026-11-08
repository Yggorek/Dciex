using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace DevEnv.CLI.Commands
{
    public class ValidateCommand
    {
        public static Command Create(IServiceProvider serviceProvider)
        {
            var command = new Command("validate", "Validate the configuration file.");
            
            return command;
        }
    }
}
