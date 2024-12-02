using AdventOfCode.Models;

namespace AdventOfCode.Solutions;

public interface ISolution
{
    public int Day { get; }
    public void Solve(SolutionType type, string[] content);
}