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
            var perimeter = grid.CalculateRegionPerimeter(y).Distinct();
            return y.Count() * (options.Value.SolutionType is SolutionType.First ? perimeter.Count() : CalculateSides(perimeter));
        })).Sum();
    }

    private enum GroupType { X, Y }
    private static int CalculateSides(IEnumerable<Point> perimeter)
    {
        var ordered = perimeter.OrderBy(p => p.X).ThenBy(p => p.Y);
        var xGroups = ordered.GroupBy(p => p.X);
        var yGroups = ordered.GroupBy(p => p.Y);

        HashSet<HashSet<Point>> allSides = [];
        HandleSides(xGroups, GroupType.X);
        HandleSides(yGroups, GroupType.Y);

        foreach (var potentialSide in allSides.ToList())
        {
            if (potentialSide.Count > 1)
                continue;

            if (allSides.Any(x => x.Count > 1 && x.Contains(potentialSide.Single())))
                allSides.Remove(potentialSide);
        }

        return allSides.Count;
        
        void HandleSides(IEnumerable<IGrouping<int, Point>> groups, GroupType type)
        {
            foreach(var group in groups)
            {
                HashSet<Point> thisSide = [];
                foreach(var (one, two) in group.Zip(group.Skip(1)))
                {
                    var diff = type is GroupType.X ? one.Y - two.Y : one.X - two.X;
                    if (Math.Abs(diff) is 1)
                    {
                        var clashPoints = allSides.Where(x => x.Count is 1 && (x.Single() == one || x.Single() == two)).ToList();
                        if (clashPoints.Count is 0)
                        {
                            foreach (var clash in clashPoints) allSides.Remove(clash);
                        }

                        thisSide.Add(one);
                        thisSide.Add(two);
                        continue;
                    }
                    else
                    {
                        if (thisSide.Count is not 0)
                        {
                            allSides.Add(thisSide);
                            thisSide = [];
                        }

                        allSides.Add([one]);
                        allSides.Add([two]);
                    }
                }

                if (thisSide.Count is not 0)
                {
                    allSides.Add(thisSide);
                }
            }
        }
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