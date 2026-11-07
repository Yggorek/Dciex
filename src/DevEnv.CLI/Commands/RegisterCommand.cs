using System;
using System.CommandLine;
using DevEnv.Application.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace DevEnv.CLI.Commands;

public class RegisterCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("register", "Register a new project.");
        var projectRegistry = serviceProvider.GetRequiredService<IProjectRegistry>();

        var pathArgument = new Argument<string>("path")
        {
            Description = "The path of the project to register."
        };
        command.Arguments.Add(pathArgument);

        command.SetAction(async execute =>
        {
            var path = execute.GetValue(pathArgument);
            try
            {
                var project = await projectRegistry.RegisterAsync(path);
                Console.WriteLine($"Project '{project.Name}' registered successfully at '{project.Path}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering project: {ex.Message}");
            }
        });

        return command;
    }
}
