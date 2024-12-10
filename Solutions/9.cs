using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day9(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 9;

    public async Task<long> Solve()
    {
        var lines = await loader.LoadGrid<char>(Day, options.Value.SolutionType, options.Value.RunType);
        var items = lines.First();

        var storageBytes = items
            .Select((ch, i) => new { Count = int.Parse(ch.ToString()), Symbol = i % 2 is 0 ? (i / 2).ToString() : "." })
            .SelectMany(x => Enumerable.Repeat(x.Symbol, x.Count))
            .ToList();

        if (options.Value.SolutionType is SolutionType.First)
        {
            while (!IsSorted(storageBytes))
            {
                var i1 = storageBytes.FindIndex(x => x is ".");
                var i2 = storageBytes.FindLastIndex(x => x is not ".");
                (storageBytes[i2], storageBytes[i1]) = (storageBytes[i1], storageBytes[i2]);
            }
        }
        else
        {
            var currentId = long.Parse(storageBytes[storageBytes.FindLastIndex(x => x is not ".")]);
            while (currentId >= 0)
            {
                var byteCount = storageBytes.Count(x => x == currentId.ToString());
                var currentIndex = storageBytes.IndexOf(currentId.ToString());

                var spaceIndex = FindSpaceIndex(storageBytes, byteCount);
                if (spaceIndex != -1 && spaceIndex < currentIndex)
                {
                    foreach (var (src, dst) in Enumerable.Range(currentIndex, byteCount).Zip(Enumerable.Range(spaceIndex, byteCount)))
                        (storageBytes[src], storageBytes[dst]) = (storageBytes[dst], storageBytes[src]);
                }

                currentId--;
            }
        }

        return storageBytes.Index().Select(x => x.Item is "." ? 0 : long.Parse(x.Item) * x.Index).Sum();
    }

    private static int FindSpaceIndex(List<string> storageBytes, int count)
    {
        var indexCounter = 0;
        foreach (var g in GroupAdjacent(storageBytes))
        {
            if (g.Key is "." && g.Value >= count)
                return indexCounter;
            indexCounter += g.Value;
        }
        return -1;
    }

    private static IEnumerable<KeyValuePair<string, int>> GroupAdjacent(IEnumerable<string> sequence)
    {
        using var iter = sequence.GetEnumerator();

        if (iter.MoveNext())
        {
            var prevItem = iter.Current;
            var runCount = 1;

            while (iter.MoveNext())
            {
                if (prevItem == iter.Current)
                {
                    ++runCount;
                }
                else
                {
                    yield return new KeyValuePair<string, int>(prevItem, runCount);
                    prevItem = iter.Current;
                    runCount = 1;
                }
            }

            yield return new KeyValuePair<string, int>(prevItem, runCount);
        }
    }

    private static bool IsSorted(IEnumerable<string> data) => data.Count() == (data.TakeWhile(x => x is not ".").Count() + data.AsEnumerable().Reverse().TakeWhile(x => x is ".").Count());
}