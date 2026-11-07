using DevEnv.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using DevEnv.CLI.Commands;
using DevEnv.Application.Projects;
using DevEnv.Infrastructure.Projects;

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

using IHost host = builder.Build();

IServiceProvider serviceProvider = host.Services;

var rootCommand = new RootCommand("DevEnv CLI");

rootCommand.Subcommands.Add(HelloCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(ListCommand.Create(serviceProvider));
rootCommand.Subcommands.Add(InfoCommand.Create(serviceProvider));

await rootCommand.Parse(args).InvokeAsync();         







