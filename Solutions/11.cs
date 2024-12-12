using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day11(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 11;

    private readonly Dictionary<(long, int), IEnumerable<long>> Cache = [];
    public async Task<long> Solve()
    {
        var data = await loader.Load<long>(Day, options.Value.SolutionType, options.Value.RunType);
        IEnumerable<long> stones = data.First();
        return ProcessBlinks(stones, [ZeroRule, DigitRule, WildcardRule], options.Value.SolutionType is SolutionType.First ? 25 : 75).Sum();
    }

    private IEnumerable<long> ProcessBlinks(IEnumerable<long> stones, List<Func<long, IEnumerable<long>>> rules, int blinkCount)
    {
        foreach (var stone in stones)
        {
            yield return Blink(stone, rules, blinkCount).Sum();
        }
    }

    private IEnumerable<long> Blink(long stone, List<Func<long, IEnumerable<long>>> rules, int blinkCount)
    {
        if (Cache.TryGetValue((stone, blinkCount), out var cacheResults))
        {
            foreach (var cacheResult in cacheResults) yield return cacheResult;
            yield break;
        }

        List<long> results = [];
        if (blinkCount is 0)
        {
            results.Add(1);
        }
        else
        {
            foreach (var rule in rules)
            {
                var result = rule.Invoke(stone);

                if (result.Count() is 1 && result.First() == stone)
                {
                    continue;
                }

                results.AddRange(result.SelectMany(x => Blink(x, rules, blinkCount - 1)));
                break;
            }
        }

        Cache.Add((stone, blinkCount), results);
        yield return results.Sum();
    }

    private static IEnumerable<long> ZeroRule(long i) => i is 0 ? [1] : [i];
    private static IEnumerable<long> DigitRule(long i)
    {
        string number = i.ToString();
        if (number.Length % 2 is 0)
        {
            var one = long.Parse(new string(number.Take(number.Length / 2).ToArray()));
            var two = long.Parse(new string(number.Skip(number.Length / 2).ToArray()));
            return [one, two];
        }
        else return [i];
    }
    private static IEnumerable<long> WildcardRule(long i) => [i * 2024];
}