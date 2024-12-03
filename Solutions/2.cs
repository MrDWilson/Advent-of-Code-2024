using AdventOfCode.Models;
using AdventOfCode.Services;
using AdventOfCode.Solutions.Base;

namespace AdventOfCode.Solutions;

public class Day2(IFileLoader _loader) : SolutionBase(_loader)
{
    public override int Day => 2;

    public override async Task Solve(SolutionType solutionType, RunType runType)
    {
        var reports = await Loader.Load<int>(Day, solutionType, runType);
        var safeCount = reports.Count(x => solutionType switch 
        {
            SolutionType.First => ReportIsSafe(x),
            SolutionType.Second => ReportIsSafe(x) || ReportIsSafeLeniant(x),
            _ => throw new ArgumentOutOfRangeException(nameof(solutionType))
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