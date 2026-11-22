using System.CommandLine;
using Dciex.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dciex.CLI.Commands;
public class HelloCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
       var helloCommand = new Command("hello", "Print greeting");

       helloCommand.SetAction(execute =>
       {
          var helloService = serviceProvider.GetRequiredService<HelloService>();
          Console.WriteLine(helloService.Execute());
       });

        return helloCommand;
    }
}