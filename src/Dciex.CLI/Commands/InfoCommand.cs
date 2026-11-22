using System.CommandLine;
using Dciex.Application.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace Dciex.CLI.Commands;

public class InfoCommand
{
 public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("info", "Show information about a project.");
        var projectRegistry = serviceProvider.GetRequiredService<IProjectRegistry>();
        var projectStatusChecker = serviceProvider.GetRequiredService<IProjectStatusChecker>();

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

            var project = await projectRegistry.GetByNameAsync(name);
            if (project == null)
            {
                Console.WriteLine($"Project '{name}' not found.");
                return;
            }

            var status = projectStatusChecker.Check(project);
            Console.WriteLine($"Project Information:");
            Console.WriteLine(new string('-', 30));
            Console.WriteLine($"Id: {project.Id}");
            Console.WriteLine($"Name: {project.Name}");
            Console.WriteLine($"Path: {project.Path}");
            Console.WriteLine($"Created At: {project.CreatedAt}");
            Console.WriteLine($"Status: {status}");
        });

        return command;
    } 
}
