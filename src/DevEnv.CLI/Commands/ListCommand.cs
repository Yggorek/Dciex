using System.CommandLine;
using DevEnv.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using DevEnv.Application.Projects;
namespace DevEnv.CLI.Commands
{
    public class ListCommand
    {
        public static Command Create(IServiceProvider serviceProvider)
        {
            var command = new Command("list", "List all projects.");
            var projectRegistry = serviceProvider.GetRequiredService<IProjectRegistry>();
            var projectStatusChecker = serviceProvider.GetRequiredService<IProjectStatusChecker>();
            command.SetAction(async execute =>
            {
                var projects = await projectRegistry.GetAllAsync();
                if (projects.Count == 0)
                {
                    Console.WriteLine("No projects registered.");
                    return;
                }
                Console.WriteLine($"{"NAME",-15} {"STATUS",-12} {"PATH"}");
                Console.WriteLine(new string('-', 70));
                foreach (var project in projects)
                {
                    var status = projectStatusChecker.Check(project);
                      Console.WriteLine($"{project.Name,-15} {status,-12} {project.Path}");
                }
            });
            return command;
        }
    }
}
