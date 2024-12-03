using AdventOfCode.Models;
using AdventOfCode.Services;
using AdventOfCode.Solutions.Base;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public class Day1(IFileLoader _loader, IOptions<SolutionOptions> _options) : SolutionBase(_loader, _options)
{
    public override int Day => 1;

    public override async Task Solve()
    {
        var lines = await Loader.Load<int>(Day, Options.SolutionType, Options.RunType);
        var listOne = lines.Select(parts => parts.First()).ToList();
        var listTwo = lines.Select(parts => parts.Last()).ToList();
        var result = Options.SolutionType switch
        {
            SolutionType.First => listOne.Order().Zip(listTwo.Order(), (x, y) => Math.Abs(x - y)).Sum(),
            SolutionType.Second => listOne.Sum(x => x * listTwo.Count(y => y == x)),
            _ => throw new ArgumentOutOfRangeException(nameof(Options.SolutionType))
        };

        Console.WriteLine(result);
    }
}