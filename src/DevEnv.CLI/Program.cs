using DevEnv.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using DevEnv.CLI.Commands;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<HelloService>();

using IHost host = builder.Build();

IServiceProvider serviceProvider = host.Services;




var rootCommand = new RootCommand("DevEnv CLI");

rootCommand.Subcommands.Add(HelloCommand.Create(serviceProvider));

rootCommand.Parse(args).Invoke();         







