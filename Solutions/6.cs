using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day6(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 6;

    private enum Direction { Up, Down, Left, Right };
    public async Task<int> Solve()
    {
        var data = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var rows = data.Select(x => x.ToCharArray().ToList()).ToList();

        int result;
        if (options.Value.SolutionType is SolutionType.First)
        {
            result = FollowPath(rows)?.Count ?? 0;
        }
        else
        {
            result = 0;
            foreach (var (rowIndex, row) in rows.Index())
            {
                foreach (var (colIndex, col) in row.Index())
                {
                    var currentItem = rows[rowIndex][colIndex];
                    if (currentItem != '#' && currentItem != '^')
                    {
                        var copy = rows
                            .Select(row => row.ToList())
                            .ToList();
                        copy[rowIndex][colIndex] = '#';

                        if (FollowPath(copy) is null)
                        {
                            result += 1;
                        }
                    }
                }
            }
        }

        return result;
    }

    private static HashSet<(int X, int Y)>? FollowPath(List<List<char>> rows)
    {
        var startRow = rows.First(x => x.Any(y => y is '^'));
        var X = rows.IndexOf(startRow);
        var Y = startRow.IndexOf('^');

        var travelledCoords = new HashSet<(int X, int Y)> { (X, Y) };
        var historyCoords = new Dictionary<(int X, int Y), List<Direction>>();
        var direction = Direction.Up;
        while (true)
        {
            var (NextX, NextY) = direction switch
            {
                Direction.Up => (X - 1, Y),
                Direction.Down => (X + 1, Y),
                Direction.Left => (X, Y - 1),
                Direction.Right => (X, Y + 1),
                _ => throw new ArgumentOutOfRangeException(nameof(direction)) 
            };

            if (NextX < 0 || NextX >= rows.Count || NextY < 0 || NextY >= startRow.Count)
            {
                break;
            }
            else if (rows[NextX][NextY] is '#')
            {
                direction = direction switch
                {
                    Direction.Up => Direction.Right,
                    Direction.Down => Direction.Left,
                    Direction.Left => Direction.Up,
                    Direction.Right => Direction.Down,
                    _ => throw new ArgumentOutOfRangeException(nameof(direction)) 
                };

                continue;
            }
            else if (historyCoords.TryGetValue((NextX, NextY), out var dir) && dir.Contains(direction))
            {
                return null; // Stuck in a loop
            }

            (X, Y) = (NextX, NextY);
            travelledCoords.Add((X, Y));

            if (historyCoords.TryGetValue((X, Y), out var currentHistoryVal))
            {
                currentHistoryVal.Add(direction);
            }
            else
            {
                historyCoords.TryAdd((X, Y), [direction]);
            }
        }

        return travelledCoords;
    }
}