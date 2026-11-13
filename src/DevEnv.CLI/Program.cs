using DevEnv.Application.Configuration;
using DevEnv.Application.Environments;
using DevEnv.Application.Environments.Abstractions;
using DevEnv.Application.Projects;
using DevEnv.Application.Services;
using DevEnv.CLI.Commands;
using DevEnv.Infrastructure.Configuration;
using DevEnv.Infrastructure.Docker.Compose;
using DevEnv.Infrastructure.Files;
using DevEnv.Infrastructure.Projects;
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
    DevEnv.Application.Configuration.IConfigurationReader>(
    _ => new DevEnv.Infrastructure.Configuration.YamlConfigurationReader());
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

var rootCommand = new RootCommand("DevEnv CLI");

rootCommand.Subcommands.Add(HelloCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(ListCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(InfoCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(RegisterCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(UnregisterCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(ValidateCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(UpCommand.Create(serviceProvider));

Environment.ExitCode = await rootCommand.Parse(args).InvokeAsync();      







