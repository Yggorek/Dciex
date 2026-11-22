foreach (var argument in args)
{
    Console.WriteLine(argument);
}
Console.Error.WriteLine("fake stderr");

var workingDirectory = Environment.CurrentDirectory;
await File.WriteAllTextAsync(
    Path.Combine(workingDirectory, "fake-docker-pid"),
    Environment.ProcessId.ToString());
var exitCodeFile = Path.Combine(workingDirectory, "fake-docker-exit-code");
var delayFile = Path.Combine(workingDirectory, "fake-docker-delay");

if (File.Exists(delayFile))
{
    await Task.Delay(TimeSpan.FromSeconds(30));
}

return File.Exists(exitCodeFile)
    ? int.Parse(await File.ReadAllTextAsync(exitCodeFile))
    : 0;
