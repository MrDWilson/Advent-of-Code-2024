using AdventOfCode.Models;
using AdventOfCode.Solutions.Base;

namespace AdventOfCode.Services;

public interface ISolutionRunner
{
    Task Run(int day, SolutionType solutionType, RunType runType);
}

public class SolutionRunner(IEnumerable<ISolution> solutions) : ISolutionRunner
{
    public async Task Run(int day, SolutionType solutionType, RunType runType)
    {
        var solution = solutions.FirstOrDefault(s => s.Day == day);
        if (solution == null)
        {
            Console.WriteLine($"Solution for day {day} not found.");
            return;
        }

        Console.WriteLine($"Running {runType} solution for day {day} {solutionType} part...");
        await solution.Solve(solutionType, runType);
    }    
}