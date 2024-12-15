using System.Drawing;
using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day15(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 15;
    
    private readonly char[] Directions = ['<', '^', '>', 'v'];

    private enum CellType { Wall, Box, Empty, Robot }
    public async Task<long> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var gridLines = lines.TakeWhile(x => !x.Any(x => Directions.Contains(x)));
        var instructionLine = string.Join("", lines.Except(gridLines).Select(x => x.Trim()));

        var grid = new Grid<CellType>(gridLines.Select(line => line.Select(GetCellType).ToList()).ToList());
        var instructions = instructionLine.Select(GetInstruction);

        foreach (var instruction in instructions)
        {
            Move(grid, instruction);
        }

        return grid.FindItems(CellType.Box).Select(GetGPSValue).Sum();
    }

    private static long GetGPSValue(Point point)
    {
        return (100 * point.X) + point.Y;
    }

    private static void Move(Grid<CellType> grid, Direction direction)
    {
        var currentPosition = grid.FindItems(CellType.Robot).Single();
        var (nextLocation, nextItem) = grid.GetAdjacentItem(currentPosition, direction);

        if (nextItem is CellType.Wall)
        {
            return;
        }
        else if (nextItem is CellType.Empty)
        {
            grid.SwapItems(currentPosition, nextLocation);
        }
        else if (nextItem is CellType.Box)
        {
            Stack<(Point one, Point two)> items = [];
            items.Push((currentPosition, nextLocation));

            var locationStore = nextLocation;
            while (nextItem is not CellType.Empty)
            {
                (nextLocation, nextItem) = grid.GetAdjacentItem(nextLocation, direction);
                if (nextItem is CellType.Wall)
                {
                    return;
                }
                else if (nextItem is CellType.Empty)
                {
                    items.Push((locationStore, nextLocation));
                    break;
                }
                else if (nextItem is CellType.Box)
                {
                    items.Push((locationStore, nextLocation));
                    locationStore = nextLocation;
                }
            }

            while (items.TryPop(out var points))
            {
                grid.SwapItems(points.one, points.two);
            }
        }
    }

    private static Direction GetInstruction(char c)
    {
        return c switch
        {
            '^' => Direction.Up,
            'v' => Direction.Down,
            '<' => Direction.Left,
            '>' => Direction.Right,
            _ => throw new ArgumentOutOfRangeException(nameof(c))
        };
    }

    private static CellType GetCellType(char c)
    {
        return c switch
        {
            '#' => CellType.Wall,
            'O' => CellType.Box,
            '@' => CellType.Robot,
            '.' => CellType.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(c))
        };
    }
}