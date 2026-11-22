using Dciex.Application.Configuration;
using Dciex.Application.Environments;
using Dciex.Application.Environments.Abstractions;
using Dciex.Application.Projects;
using Dciex.Application.Services;
using Dciex.CLI.Commands;
using Dciex.Infrastructure.Configuration;
using Dciex.Infrastructure.Docker.Compose;
using Dciex.Infrastructure.Files;
using Dciex.Infrastructure.Projects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
string appDataPath = Environment.GetFolderPath(
    Environment.SpecialFolder.LocalApplicationData);

string registryPath = Path.Combine(
    appDataPath,
    "DevEnv",
    "projects.json");

builder.Services.AddSingleton<IProjectStorage>(
    _ => new JsonProjectStorage(registryPath));
builder.Services.AddTransient<HelloService>();
builder.Services.AddSingleton<IProjectRegistry, ProjectRegistry>();
builder.Services.AddSingleton<IProjectStatusChecker, ProjectStatusChecker>();
builder.Services.AddTransient<
    Dciex.Application.Configuration.IConfigurationReader>(
    _ => new Dciex.Infrastructure.Configuration.YamlConfigurationReader());
builder.Services.AddTransient<ConfigurationLoader>();
builder.Services.AddTransient<ConfigurationValidator>();
builder.Services.AddTransient<ComposeGenerator>();
builder.Services.AddTransient<ComposeSerializer>();
builder.Services.AddTransient<IComposeFileGenerator, ComposeFileGenerator>(); 
builder.Services.AddTransient<IFileManager, FileManager>();
builder.Services.AddTransient<IDockerComposeManager, DockerComposeManager>();
builder.Services.AddTransient<EnvironmentOrchestrator>();
using IHost host = builder.Build();

IServiceProvider serviceProvider = host.Services;
serviceProvider.GetRequiredService<EnvironmentOrchestrator>();

var rootCommand = new RootCommand("Dciex CLI");

rootCommand.Subcommands.Add(HelloCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(ListCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(InfoCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(RegisterCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(UnregisterCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(ValidateCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(UpCommand.Create(serviceProvider));

Environment.ExitCode = await rootCommand.Parse(args).InvokeAsync();      







