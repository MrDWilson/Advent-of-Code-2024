using AdventOfCode.Models;

namespace AdventOfCode.Solutions;

public class Day2 : ISolution
{
    public int Day => 2;

    public void Solve(SolutionType type, string[] content)
    {
        var reports = content.Select(x => x.Split(" ").Select(y => int.Parse(y)));
        if (type is SolutionType.First)
        {
            Console.WriteLine(reports.Count(x => ReportIsSafe(x)));
        }
        else
        {
            Console.WriteLine(reports.Count(x => ReportIsSafeLevelCheck(x)));
        }
    }

    private enum Direction { Increasing, Decreasing };
    private bool ReportIsSafe(IEnumerable<int> report)
    {
        Direction? direction = null;

        int last = report.First();
        foreach (var reportItem in report.Skip(1))
        {
            Direction thisDirection;

            if (reportItem < last) thisDirection = Direction.Decreasing;
            else if (reportItem > last) thisDirection = Direction.Increasing;
            else return false;

            if (direction is null) direction = thisDirection;
            else if (direction != thisDirection) return false;

            if (Math.Abs(last - reportItem) > 3) return false;

            last = reportItem;
        }
        
        return true;
    }

    private bool ReportIsSafeLevelCheck(IEnumerable<int> report)
    {
        if (ReportIsSafe(report)) return true;

        var reportItems = report.ToList();
        var reportsWithoutLevel = Enumerable.Repeat(reportItems, reportItems.Count).Select((x, i) =>
        {
            var copy = x.ToList();
            copy.RemoveAt(i);
            return copy;
        });

        return reportsWithoutLevel.Any(x => ReportIsSafe(x));
    }
}