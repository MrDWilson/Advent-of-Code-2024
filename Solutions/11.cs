using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day11(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 11;

    public async Task<long> Solve()
    {
        var data = await loader.Load<long>(Day, options.Value.SolutionType, options.Value.RunType);
        List<long> stones = data.First();
        foreach (var i in Enumerable.Range(0, options.Value.SolutionType is SolutionType.First ? 25 : 75))
        {
            Console.WriteLine($"{i}:{stones.Count}");
            stones = ProcessBlink(stones, [ZeroRule, DigitRule, WildcardRule]).ToList();
        }

        return stones.Count;
    }

    private static IEnumerable<long> ProcessBlink(List<long> stones, List<Func<long, IEnumerable<long>>> rules)
    {
        foreach (var stone in stones)
        {
            foreach (var rule in rules)
            {
                var result = rule.Invoke(stone);

                if (result.Count() is 1 && result.First() == stone)
                {
                    continue;
                }

                foreach (var number in result) yield return number;
                break;
            }
        }
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