using AdventOfCode.Models;
using AdventOfCode.Services;
using AdventOfCode.Solutions.Base;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public class Day2(IFileLoader _loader, IOptions<SolutionOptions> _options) : SolutionBase(_loader, _options)
{
    public override int Day => 2;

    public override async Task Solve()
    {
        var reports = await Loader.Load<int>(Day, Options.SolutionType, Options.RunType);
        var safeCount = reports.Count(x => Options.SolutionType switch 
        {
            SolutionType.First => ReportIsSafe(x),
            SolutionType.Second => ReportIsSafe(x) || ReportIsSafeLeniant(x),
            _ => throw new ArgumentOutOfRangeException(nameof(Options.SolutionType))
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