using AdventOfCode.Models;

namespace AdventOfCode.Solutions;

public abstract class SolutionBase : ISolution
{
    public abstract int Day { get; }
    public abstract void Solve(SolutionType type, string[] content);

    private protected static List<int[]> ToIntArray(ICollection<string> lines)
    {
        return lines.Select(line => line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)
                                       .Select(int.Parse)
                                       .ToArray()).ToList();
    }
}