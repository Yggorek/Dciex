
using System.CommandLine;
using DevEnv.Application.Environments;
using Microsoft.Extensions.DependencyInjection;

namespace DevEnv.CLI.Commands;

public class UpCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("up", "Start a project environment.");

        var orchestrator =
            serviceProvider.GetRequiredService<EnvironmentOrchestrator>();

        var nameArgument = new Argument<string>("name")
        {
            Description = "The name of the project."
        };

        command.Arguments.Add(nameArgument);

        command.SetAction(async execute =>
        {
            var name = execute.GetValue(nameArgument);

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Please provide a project name.");
                return;
            }

            var result = await orchestrator.UpAsync(name);

            if (result.IsSuccess)
            {
                Console.WriteLine(result.Message);
            }
            else
            {
                Console.WriteLine($"Error: {result.Message}");
            }
        });

        return command;
    }
}
