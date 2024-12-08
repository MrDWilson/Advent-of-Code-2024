using System.Drawing;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day8(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 8;

    public async Task<long> Solve()
    {
        var data = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var rows = data.Select(x => x.ToCharArray().ToList()).ToList();
        var antennas = rows.SelectMany(x => x).Where(x => x is not '.').GroupBy(x => x).Where(x => x.Count() > 1);
        var coords = antennas.ToDictionary(x => x.Key, x => GetCoords(rows, x.Key));
        
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
                    foreach (var validAntinode in antinodes.Where(x => !OutOfBounds(x, rows)))
                    {
                        allAntinodes.Add(validAntinode);
                    }
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

                    foreach (var everyAntenna in coords.SelectMany(x => x.Value).ToHashSet())
                    {
                        allAntinodes.Add(everyAntenna);
                    }
                }
            }
        }

        return allAntinodes.Count;
    }

    private static IEnumerable<Point> GetCoords(List<List<char>> rows, char c)
    {
        foreach (var (rowIndex, row) in rows.Index())
        {
            foreach (var (colIndex, col) in row.Index())
            {
                if (col == c)
                {
                    yield return new(rowIndex, colIndex);
                }
            }
        }
    }

    private static (Point one, Point two) GetAntinodeDistances(Point one, Point two)
        => (new Point(one.X - two.X, one.Y - two.Y), new Point(two.X - one.X, two.Y - one.Y));

    private static bool OutOfBounds(Point coords, List<List<char>> rows)
        => coords.X < 0 || coords.Y < 0 || coords.X >= rows.Count || coords.Y >= rows.First().Count;

    private static IEnumerable<(Point First, Point Second)> GetPairings(IEnumerable<Point> items) =>
        items.SelectMany((item, index) => items.Skip(index + 1), (first, second) => (first, second));
}