using AdventOfCode.Models;

namespace AdventOfCode.Solutions;

public interface ISolution
{
    int Day { get; }
    void Solve(SolutionType type, string[] content);
}