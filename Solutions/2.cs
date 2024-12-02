using AdventOfCode.Models;
using AdventOfCode.Solutions.Base;

namespace AdventOfCode.Solutions;

public class Day2 : SolutionBase
{
    public override int Day => 2;

    public override void Solve(SolutionType type, string[] content)
    {
        var reports = ToIntArray(content);
        var safeCount = reports.Count(x => type switch 
        {
            SolutionType.First => ReportIsSafe(x),
            SolutionType.Second => ReportIsSafe(x) || ReportIsSafeLeniant(x),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        });

        Console.WriteLine(safeCount);
    }

    private static bool IsStrictlyMonotonic(IEnumerable<int> numbers)
    {
        return numbers.Zip(numbers.Skip(1), (a, b) => b > a).All(x => x)
            || numbers.Zip(numbers.Skip(1), (a, b) => b < a).All(x => x);
    }

    private static bool ReportIsSafe(IEnumerable<int> report)
    {
        if (!IsStrictlyMonotonic(report)) return false;
        return report.Zip(report.Skip(1), (x, y) => Math.Abs(x - y) <= 3).All(x => x);
    }

    private static bool ReportIsSafeLeniant(IEnumerable<int> report)
    {
        var reportsWithoutLevel = Enumerable.Range(0, report.Count())
            .Select(i => report.Where((_, index) => index != i).ToList())
            .ToList();

        return reportsWithoutLevel.Any(x => ReportIsSafe(x));
    }
}