using AdventOfCode.Models;
using AdventOfCode.Solutions;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Services;

public interface ISolutionRunner
{
    Task Run();
    Task RunAll();
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

    public async Task RunAll()
    {
        var solutionsByDay = solutions.OrderBy(s => s.Day);

        foreach (var solution in solutionsByDay)
        {
            options.Value.SolutionType = SolutionType.First;
            Console.WriteLine($"Day: {solution.Day}, Solution 1: {await solution.Solve()}");

            options.Value.SolutionType = SolutionType.Second;
            Console.WriteLine($"Day: {solution.Day}, Solution 2: {await solution.Solve()}");
        }
    }
}