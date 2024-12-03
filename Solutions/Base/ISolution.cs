using AdventOfCode.Models;

namespace AdventOfCode.Solutions.Base;

public interface ISolution
{
    public int Day { get; }
    public Task Solve(SolutionType solutionType, RunType runType);
}