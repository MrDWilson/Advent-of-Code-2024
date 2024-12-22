using System.Collections.Concurrent;
using System.Collections.Immutable;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day22(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 22;

    public async Task<string> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var numbers = lines.Select(long.Parse);

        if (options.Value.SolutionType is SolutionType.First)
        {
            foreach (var _ in Enumerable.Range(0, 2000))
            {
                numbers = numbers.Select(GetNext);
            }

            return numbers.Sum().ToString();
        }
        else
        {
            Dictionary<long, List<(long price, long change)>> history = [];
            Dictionary<long, string> lookup = [];
            foreach (var i in numbers)
            {
                history[i] = [];
                long nextNumber = i;
                long lastNumber = i;
                long nextPrice = 0;
                long lastPrice = long.Parse(nextNumber.ToString().Last().ToString());
                foreach (var _ in Enumerable.Range(0, 2000))
                {
                    nextNumber = GetNext(lastNumber);
                    nextPrice = long.Parse(nextNumber.ToString().Last().ToString());
                    history[i].Add((nextPrice, nextPrice - lastPrice));
                    lastNumber = nextNumber;
                    lastPrice = nextPrice;
                }

                lookup[i] = string.Join(",", history[i].Select(x => x.change));
            }

            HashSet<ImmutableArray<long>> sequences = [];
            foreach (var historyItem in history)
            {
                int skip = 0;
                while (skip + 4 < historyItem.Value.Count)
                {
                    sequences.Add(historyItem.Value[skip..(skip+4)].Select(x => x.change).ToImmutableArray());
                    skip++;
                }
            }

            ConcurrentBag<long> bananaCounts = [];
            Parallel.ForEach(sequences, sequence => 
            {
                var count = GetBananaCount(history, lookup, sequence);
                bananaCounts.Add(count);
            });

            return bananaCounts.Max().ToString();
        }
    }

    private static readonly ConcurrentDictionary<string, long> _cache = [];
    private static long GetBananaCount(Dictionary<long, List<(long price, long change)>> history, Dictionary<long, string> lookup, ImmutableArray<long> changeSequence)
    {
        var key = string.Join(",", changeSequence);
        if (_cache.TryGetValue(key, out var value)) return value;

        List<long> prices = [];
        foreach (var historyItem in history.Where(x => lookup[x.Key].Contains(string.Join(",", changeSequence))))
        {
            int count = historyItem.Value.Count;
            for (int i = 0; i <= count - 4; i++)
            {
                if (historyItem.Value[i].change == changeSequence[0] &&
                    historyItem.Value[i + 1].change == changeSequence[1] &&
                    historyItem.Value[i + 2].change == changeSequence[2] &&
                    historyItem.Value[i + 3].change == changeSequence[3])
                {
                    prices.Add(historyItem.Value[i + 3].price);
                    break;
                }
            }
        }

        return _cache[key] = prices.Sum();
    }

    private static long GetNext(long value) => Op3(Op2(Op1(value)));
    private static long Op1(long value) => Prune(Mix(value * 64, value));
    private static long Op2(long value) => Prune(Mix((long)Math.Floor((double)value / 32), value));
    private static long Op3(long value) => Prune(Mix(value * 2048, value));
    private static long Mix(long value, long secret) => value ^ secret;
    private static long Prune(long value) => value % 16777216;
}