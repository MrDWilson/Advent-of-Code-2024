using AdventOfCode.Models;
using AdventOfCode.Services;
using AdventOfCode.Solutions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<IFileLoader, FileLoader>();
builder.Services.AddTransient<ISolutionRunner, SolutionRunner>();

var solutionTypes = AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => a.GetTypes())
    .Where(t => typeof(ISolution).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

foreach (var solutionType in solutionTypes)
{
    builder.Services.AddTransient(typeof(ISolution), solutionType);
}

using IHost host = builder.Build();

var runner = host.Services.GetRequiredService<ISolutionRunner>();

await runner.Run(1, SolutionType.First, RunType.Full);