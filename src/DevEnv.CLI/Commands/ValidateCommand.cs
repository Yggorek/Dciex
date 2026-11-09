using DevEnv.Application.Configuration;
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using YamlDotNet.Core;

namespace DevEnv.CLI.Commands
{
    public class ValidateCommand
    {
        public static Command Create(IServiceProvider serviceProvider)
        {
            var command = new Command("validate", "Validate the configuration file.");
            var configFileArgument = new Argument<string>("configFile")
            {
                Description = "The path to the configuration file."
            };
            command.Arguments.Add(configFileArgument);
            command.SetAction(async (execute, cancellationToken) =>
            {
                var configFilePath = execute.GetValue(configFileArgument);
                    if (string.IsNullOrWhiteSpace(configFilePath))
                {
                  Console.Error.WriteLine("Configuration file path is required.");
                  return 1;
                }
                var loader = serviceProvider.GetRequiredService<ConfigurationLoader>();
                
                try
                {
                    var validationErrors = await loader.LoadAsync(configFilePath, cancellationToken);
                    if(validationErrors.IsValid)
                {
                    Console.WriteLine("Configuration is valid.");
                    return 0;
                }
                else
                {
                    Console.Error.WriteLine("Configuration validation failed:");
                    Console.Error.WriteLine();
                    foreach (var error in validationErrors.ValidationErrors)
                    {
                        
                        Console.Error.WriteLine($"- {error.PropertyName}: {error.ErrorMessage}");
                    }
                    return 1;
                }
                }
                catch(FileNotFoundException ex)
                {
                    Console.Error.WriteLine($"Configuration file not found: {ex.Message}");
                    return 1;
                }
                catch(IOException ex)
                {
                    Console.Error.WriteLine($"Error reading configuration file: {ex.Message}");
                    return 1;
                }
                catch(UnauthorizedAccessException ex)
                {
                    Console.Error.WriteLine($"Access denied to configuration file: {ex.Message}");
                    return 1;
                }
                catch(YamlException ex)
                {
                    Console.Error.WriteLine($"Error parsing configuration file:\n Line:{ex.Start.Line} \n Column:{ex.Start.Column} \n Reason: {ex.Message}");
                    return 1;
                }
                catch(OperationCanceledException)
                {
                    Console.Error.WriteLine("Operation was canceled. ");
                    return 1;
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine($"Error loading configuration file: {ex.Message}");
                    return 1;
                }
                

                
                
            });
            return command;
        }
    }
}
