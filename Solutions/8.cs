using System.Drawing;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day8(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 8;

    public async Task<string> Solve()
    {
        var rows = await loader.LoadGrid<char>(Day, options.Value.SolutionType, options.Value.RunType);
        var coords = rows
            .SelectMany((row, rowIndex) => row.Select((c, colIndex) => (c, rowIndex, colIndex)))
            .Where(x => x.c is not '.')
            .GroupBy(x => x.c)
            .Where(g => g.Count() > 1)
            .ToDictionary(
                g => g.Key, 
                g => g.Select(x => new Point(x.rowIndex, x.colIndex)).ToList()
            );
        
        HashSet<Point> allAntinodes = [];
        foreach (var positions in coords.Values)
        {
            var allPairs = GetPairings(positions);
            foreach (var (first, second) in allPairs)
            {
                var (firstDistance, secondDistance) = GetAntinodeDistances(first, second);
                List<Point> antinodes = [
                    new Point(first.X + firstDistance.X, first.Y + firstDistance.Y), 
                    new Point(second.X + secondDistance.X, second.Y + secondDistance.Y)
                ];

                if (options.Value.SolutionType is SolutionType.First)
                {
                    allAntinodes.UnionWith(antinodes.Where(a => !OutOfBounds(a, rows)));
                }
                else
                {
                    var distance = firstDistance;
                    foreach (var antinode in antinodes)
                    {
                        var currentAntinode = antinode;
                        while (!OutOfBounds(currentAntinode, rows))
                        {
                            allAntinodes.Add(currentAntinode);
                            currentAntinode = new Point(currentAntinode.X + distance.X, currentAntinode.Y + distance.Y);
                        }

                        distance = secondDistance;
                    }

                    allAntinodes.UnionWith(coords.Values.SelectMany(v => v));
                }
            }
        }

        return allAntinodes.Count.ToString();
    }

    private static (Point one, Point two) GetAntinodeDistances(Point one, Point two)
        => (new Point(one.X - two.X, one.Y - two.Y), new Point(two.X - one.X, two.Y - one.Y));

    private static bool OutOfBounds(Point coords, List<List<char>> rows)
        => coords.X < 0 || coords.Y < 0 || coords.X >= rows.Count || coords.Y >= rows.First().Count;

    private static IEnumerable<(Point First, Point Second)> GetPairings(IEnumerable<Point> items) =>
        items.SelectMany((item, index) => items.Skip(index + 1), (first, second) => (first, second));
}