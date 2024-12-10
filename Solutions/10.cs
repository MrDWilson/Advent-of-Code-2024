using System.Drawing;
using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day10(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 10;

    public async Task<long> Solve()
    {
        var grid = await loader.LoadTypedGrid<int>(Day, options.Value.SolutionType, options.Value.RunType);
        var trailheads = grid.FindItems(0);
        return trailheads.Select(x => FollowTrailhead(grid, x)).Sum();
    }

    private long FollowTrailhead(Grid<int> grid, Point point)
    {
        return options.Value.SolutionType is SolutionType.First 
            ? FollowPath(grid, point, 1).Distinct().Count()
            : FollowPath(grid, point, 1).Count();
    }

    private static IEnumerable<Point> FollowPath(Grid<int> grid, Point point, int nextValue)
    {
        var matchingItems = grid.GetSurroundingItems(point).Where(x => grid[x] == nextValue);

        if (!matchingItems.Any())
        {
            yield break;
        }
        else
        {
            foreach (var item in matchingItems)
            {
                if (nextValue is 9) yield return item;
                else
                {
                    foreach (var nextPath in FollowPath(grid, item, nextValue + 1))
                        yield return nextPath;
                }
            }
        }
    }
}