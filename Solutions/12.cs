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
        var plantAreas = plants.Select(x => GetAreas(grid, x));
        return plantAreas.SelectMany(x => x.Select(y => 
        {
            var perimeter = grid.CalculateRegionPerimeter(y).Distinct();
            return y.Count() * (options.Value.SolutionType is SolutionType.First ? perimeter.Count() : CalculateSides(perimeter));
        })).Sum();
    }

    private static int CalculateSides(IEnumerable<Point> perimeter)
    {
        double cx = perimeter.Average(p => p.X);
        double cy = perimeter.Average(p => p.Y);
        var orderedEdges = perimeter
            .OrderBy(p => Math.Atan2(p.Y - cy, p.X - cx))
            .ToList();

        int sides = 0;
        var listLooped = orderedEdges.Skip(1).ToList();
        listLooped.Add(orderedEdges.First());

        Point? justTurned = null;
        foreach (var (edgeOne, edgeTwo) in orderedEdges.Zip(listLooped))
        {
            if (edgeOne.X != edgeTwo.X && edgeOne.Y != edgeTwo.Y)
            {
                if (justTurned is not null && Math.Abs(justTurned.Value.X - edgeTwo.X) is 2 && Math.Abs(justTurned.Value.Y - edgeTwo.Y) is 2)
                {
                    sides += 2;
                }
                else sides++;

                justTurned = edgeOne;
            }
            else justTurned = null;
        }

        return sides;
    }

    private static List<Point> GrahamScan(IEnumerable<Point> points)
    {
        var sorted = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
        List<Point> hull = new List<Point>();

        // Build lower hull
        foreach (var p in sorted)
        {
            while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }

        // Build upper hull
        for (int i = sorted.Count - 2, l = hull.Count + 1; i >= 0; i--)
        {
            while (hull.Count >= l && Cross(hull[hull.Count - 2], hull[hull.Count - 1], sorted[i]) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(sorted[i]);
        }

        hull.RemoveAt(hull.Count - 1);
        return hull;
    }

    private static double Cross(Point a, Point b, Point c) 
        => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);

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