using AdventOfCode.Models;
using AdventOfCode.Services;
using AdventOfCode.Solutions.Base;

namespace AdventOfCode.Solutions;

public class Day1(IFileLoader _loader) : SolutionBase(_loader)
{
    public override int Day => 1;

    public override async Task Solve(SolutionType solutionType, RunType runType)
    {
        var lines = await Loader.Load<int>(Day, solutionType, runType);
        var listOne = lines.Select(parts => parts.First()).ToList();
        var listTwo = lines.Select(parts => parts.Last()).ToList();
        var result = solutionType switch
        {
            SolutionType.First => listOne.Order().Zip(listTwo.Order(), (x, y) => Math.Abs(x - y)).Sum(),
            SolutionType.Second => listOne.Sum(x => x * listTwo.Count(y => y == x)),
            _ => throw new ArgumentOutOfRangeException(nameof(solutionType))
        };

        Console.WriteLine(result);
    }
}