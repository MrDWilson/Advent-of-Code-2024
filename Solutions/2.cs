using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public class Day2(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 2;

    public async Task<string> Solve()
    {
        var reports = await loader.Load<int>(Day, options.Value.SolutionType, options.Value.RunType);
        var safeCount = reports.Count(x => options.Value.SolutionType switch 
        {
            SolutionType.First => ReportIsSafe(x),
            SolutionType.Second => ReportIsSafe(x) || ReportIsSafeLeniant(x),
            _ => throw new ArgumentOutOfRangeException(nameof(options.Value.SolutionType))
        });

        return safeCount.ToString();
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