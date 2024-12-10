using System.Drawing;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day10(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 10;

    public async Task<long> Solve()
    {
        var grid = await loader.LoadGrid<int>(Day, options.Value.SolutionType, options.Value.RunType);
        var trailheads = grid.Index().SelectMany(x => x.Item.Index().Select(y => (Point: new Point(x.Index, y.Index), y.Item))).Where(x => x.Item is 0);

        return trailheads.Select(x => FollowTrailhead(grid, x.Point)).Sum();
    }

    private long FollowTrailhead(List<List<int>> grid, Point point)
    {
        return options.Value.SolutionType is SolutionType.First 
            ? FollowPath(grid, point, 1).Distinct().Count()
            : FollowPath(grid, point, 1).Count();
    }

    private static IEnumerable<Point> FollowPath(List<List<int>> grid, Point point, int nextValue)
    {
        var matchingItems = GetSurroundingItems(point).Where(x => !OutOfBounds(x, grid)).Where(x => grid[x.X][x.Y] == nextValue);

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

    private static IEnumerable<Point> GetSurroundingItems(Point point)
    {
        return 
        [
            new Point(point.X - 1, point.Y), 
            new Point(point.X + 1, point.Y), 
            new Point(point.X, point.Y - 1), 
            new Point(point.X, point.Y + 1)
        ];
    }

    private static bool OutOfBounds<T>(Point coords, List<List<T>> rows)
        => coords.X < 0 || coords.Y < 0 || coords.X >= rows.Count || coords.Y >= rows.First().Count;
}