using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day19(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 19;

    private enum Colour { White, Blue, Black, Red, Green };
    public async Task<string> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var towels = lines.First().Split(",").Select(x => x.Trim()).Select(x => x.Select(ParseColour).ToList()).ToList();
        var designs = lines.Skip(1).Select(x => x.Trim()).Select(x => x.Select(ParseColour).ToList()).ToList();

        var matchingDesigns = designs.Select(x => IsDesignPossible(towels, x));
        return options.Value.SolutionType is SolutionType.First 
            ? matchingDesigns.Count(x => x > 0).ToString()
            : matchingDesigns.Sum().ToString();
    }

    private static readonly Dictionary<string, long> Memo = [];
    private static long IsDesignPossible(List<List<Colour>> towels, List<Colour> design)
    {
        if (design.Count is 0) return 1;

        var key = string.Join(",", design);
        if (Memo.TryGetValue(key, out long cached)) return cached;

        long ways = 0;
        foreach (var towel in towels)
        {
            int len = towel.Count;
            if (len <= design.Count && design.Take(len).SequenceEqual(towel))
            {
                ways += IsDesignPossible(towels, design.Skip(len).ToList());
            }
        }

        Memo[key] = ways;
        return ways;
    }

    private static Colour ParseColour(char c)
    {
        return c switch
        {
            'w' => Colour.White,
            'u' => Colour.Blue,
            'b' => Colour.Black,
            'r' => Colour.Red,
            'g' => Colour.Green,
            _ => throw new ArgumentOutOfRangeException(nameof(c))
        };
    }
}