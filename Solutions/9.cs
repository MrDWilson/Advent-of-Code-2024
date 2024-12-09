using System.Security.Cryptography.X509Certificates;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day9(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 9;

    private enum StorageType { Data, Empty };
    public async Task<long> Solve()
    {
        var lines = await loader.LoadGrid(Day, options.Value.SolutionType, options.Value.RunType);
        var items = lines.First();

        var storageBytes = items.Index()
            .Select(x => (Item: int.Parse(x.Item.ToString()), Type: x.Index % 2 is not 0 ? StorageType.Empty : StorageType.Data))
            .Index()
            .SelectMany(x => 
            {
                return Enumerable.Repeat(x.Item.Type switch
                {
                    StorageType.Empty => ".",
                    StorageType.Data => (x.Index / 2).ToString(),
                    _ => throw new ArgumentOutOfRangeException(nameof(x.Item.Type))
                }, x.Item.Item);
            })
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
            var currentId = int.Parse(storageBytes[storageBytes.FindLastIndex(x => x is not ".")]);
            while (currentId >= 0)
            {
                var byteCount = storageBytes.Count(x => x == currentId.ToString());

                var currentIndex = storageBytes.IndexOf(currentId.ToString());
                var spaceIndex = new string(storageBytes.SelectMany(x => x).ToArray()).IndexOf(new string(Enumerable.Repeat('.', byteCount).ToArray()));

                if (spaceIndex is not -1 && spaceIndex < currentIndex)
                {
                    var currentIndexes = Enumerable.Range(currentIndex, byteCount);
                    var spaceIndexes = Enumerable.Range(spaceIndex, byteCount);
                    foreach (var (ci, si) in currentIndexes.Zip(spaceIndexes))
                    {
                        (storageBytes[ci], storageBytes[si]) = (storageBytes[si], storageBytes[ci]);
                    }
                }

                currentId--;
            }
        }

        return storageBytes.Index().Select(x => x.Item is "." ? 0 : long.Parse(x.Item) * x.Index).Sum();
    }

    private static bool IsSorted(IEnumerable<string> data) => data.Count() == (data.TakeWhile(x => x is not ".").Count() + data.AsEnumerable().Reverse().TakeWhile(x => x is ".").Count());
}