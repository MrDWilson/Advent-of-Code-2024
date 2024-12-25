using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day25(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 25;

    public async Task<string> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var blocks = lines.Chunk(7);

        var keyBlocks = blocks.Where(x => x.Last().All(x => x is '#'));
        var lockBlocks = blocks.Where(x => x.First().All(x => x is '#'));

        var keys = keyBlocks.Select(GetCounts);
        var locks = lockBlocks.Select(GetCounts);

        var max = keyBlocks.First().Length - 1;

        int countMatch = 0;
        foreach (var @lock in locks)
        {
            foreach (var key in keys)
            {
                if (@lock.Zip(key).All(x => x.First + x.Second < max))
                {
                    countMatch++;
                }
            }
        }

        return countMatch.ToString();
    }

    private static IEnumerable<int> GetCounts(string[] block)
    {
        foreach (var column in Enumerable.Range(0, block.First().Length))
        {
            yield return block.Count(x => x[column] is '#') - 1;
        }
    }
}