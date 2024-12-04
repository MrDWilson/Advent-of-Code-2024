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

        Console.WriteLine($"Day {options.Value.Day}, {options.Value.SolutionType} part, {options.Value.RunType} run");
        Console.WriteLine($"https://adventofcode.com/2024/day/{options.Value.Day}");
        Console.WriteLine($"Solution: {await solution.Solve()}");
    }    
}