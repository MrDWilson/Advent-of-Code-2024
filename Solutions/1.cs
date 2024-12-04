using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public class Day1(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 1;

    public async Task Solve()
    {
        var lines = await loader.Load<int>(Day, options.Value.SolutionType, options.Value.RunType);
        var listOne = lines.Select(parts => parts.First()).ToList();
        var listTwo = lines.Select(parts => parts.Last()).ToList();
        var result = options.Value.SolutionType switch
        {
            SolutionType.First => listOne.Order().Zip(listTwo.Order(), (x, y) => Math.Abs(x - y)).Sum(),
            SolutionType.Second => listOne.Sum(x => x * listTwo.Count(y => y == x)),
            _ => throw new ArgumentOutOfRangeException(nameof(options.Value.SolutionType))
        };

        Console.WriteLine(result);
    }
}