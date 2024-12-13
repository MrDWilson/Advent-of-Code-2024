using System.Drawing;
using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day12(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 12;

    public async Task<long> Solve()
    {
        var grid = await loader.LoadTypedGrid<char>(Day, options.Value.SolutionType, options.Value.RunType);
        var plants = grid.GetUniqueItems();
        var plantAreas = plants.Select(x => (Plant: x, Area: GetAreas(grid, x)));
        return plantAreas.SelectMany(x => x.Area.Select(y => 
        {
            var perimeter = grid.CalculateRegionPerimeter(y);
            return y.Count() * (options.Value.SolutionType is SolutionType.First ? perimeter.Count() : CountSides(y).Sum());
        })).Sum();
    }

    private enum Direction { Up, Down, Left, Right }
    private enum Axis { X, Y }
    private static IEnumerable<int> CountSides(IEnumerable<Point> area)
    {
        yield return area.GroupBy(p => p.X).OrderBy(p => p.Key)
            .Select(p => 
            {
                var upSides = CountSidesDirectional(area, p, Direction.Up, Axis.X);
                var downSides = CountSidesDirectional(area, p, Direction.Down, Axis.X);
                return upSides + downSides;
            }).Sum();

        yield return area.GroupBy(p => p.Y).OrderBy(p => p.Key)
            .Select(p => 
            {
                var leftSides = CountSidesDirectional(area, p, Direction.Left, Axis.Y);
                var rightSides = CountSidesDirectional(area, p, Direction.Right, Axis.Y);
                return leftSides + rightSides;
            }).Sum();
    }

    private static int CountSidesDirectional(
        IEnumerable<Point> area,
        IGrouping<int, Point> group, 
        Direction direction, 
        Axis axis
    )
    {
        var row = group.OrderBy(p => axis is Axis.X ? p.Y : p.X);
        Point checkPoint = (axis, direction) switch
        {
            (Axis.X, Direction.Up) => new(-1, 0),
            (Axis.X, Direction.Down) => new(1, 0),
            (Axis.Y, Direction.Left) => new(0, -1),
            (Axis.Y, Direction.Right) => new(0, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };

        var exposedPoints = row.Where(p => !area.Contains(new Point(p.X + checkPoint.X, p.Y + checkPoint.Y)));
        int sides = exposedPoints.Any() ? 1 : 0;
        foreach (var (one, two) in exposedPoints.Zip(exposedPoints.Skip(1)))
        {
            var xDiff = Math.Abs(one.X - two.X);
            var yDiff = Math.Abs(one.Y - two.Y);
            if ((axis is Axis.X ? yDiff : xDiff) is 1) continue;
            sides++;
        }
        
        return sides;
    }

    private static List<IEnumerable<Point>> GetAreas(Grid<char> grid, char c)
    {
        var points = grid.FindItems(c);
        List<IEnumerable<Point>> areas = [];
        foreach (var point in points)
        {
            if (areas.Any(x => x.Contains(point)))
            {
                continue;
            }

            var allPoints = grid.FindConnectedRegion(point);
            areas.Add(allPoints);
        }

        return areas;
    }
}