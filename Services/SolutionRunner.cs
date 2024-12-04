using AdventOfCode.Models;
using AdventOfCode.Solutions;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Services;

public interface ISolutionRunner
{
    Task Run();
}

public class SolutionRunner(IEnumerable<ISolution> solutions, IOptions<SolutionOptions> options) : ISolutionRunner
{
    public async Task Run()
    {
        var solution = solutions.FirstOrDefault(s => s.Day == options.Value.Day);
        if (solution == null)
        {
            Console.WriteLine($"Solution for day {options.Value.Day} not found.");
            return;
        }

        Console.WriteLine($"Running {options.Value.RunType} solution for day {options.Value.Day} {options.Value.SolutionType} part...");
        await solution.Solve();
    }    
}