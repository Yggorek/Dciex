using System;
using System.CommandLine;
using DevEnv.Application.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace DevEnv.CLI.Commands;

public class UnregisterCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("unregister", "Unregister project.");
        var projectRegistry = serviceProvider.GetRequiredService<IProjectRegistry>();

        var nameArgument = new Argument<string>("name")
        {
            Description = "The name of the project to unregister."
        };
        command.Arguments.Add(nameArgument);

        command.SetAction(async execute =>
        {
            var name = execute.GetValue(nameArgument);
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                  return;
                }
                var success = await projectRegistry.UnregisterAsync(name);
                if (success)
                {
                    Console.WriteLine($"Project '{name}' unregistered successfully.");
                }
                else
                {
                    Console.WriteLine($"Project '{name}' not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unregistering project: {ex.Message}");
            }
        });

        return command;
    }
}
