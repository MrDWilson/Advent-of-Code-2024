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

    private enum CellType { Wall, Box, BoxLeft, BoxRight, Empty, Robot }
    public async Task<string> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var gridLines = lines.TakeWhile(x => !x.Any(x => Directions.Contains(x)));
        var instructionLine = string.Join("", lines.Except(gridLines).Select(x => x.Trim()));

        var gridLists = gridLines.Select(line => line.Select(GetCellType).ToList()).ToList();
        if (options.Value.SolutionType is SolutionType.Second)
        {
            gridLists = DoubleGrid(gridLists);
        }

        var grid = new Grid<CellType>(gridLists);
        var instructions = instructionLine.Select(GetInstruction);

        foreach (var instruction in instructions)
        {
            Move(grid, instruction);
        }

        return options.Value.SolutionType is SolutionType.First
            ? grid.FindItems(CellType.Box).Select(GetGPSValue).Sum().ToString()
            : grid.FindItems(CellType.BoxLeft).Select(GetGPSValue).Sum().ToString();
    }

    private static List<List<CellType>> DoubleGrid(List<List<CellType>> grid)
    {
        List<List<CellType>> newGrid = [];
        foreach (var row in grid)
        {
            List<CellType> newRow = [];
            foreach (var col in row)
            {
                List<CellType> newItems = col switch
                {
                    CellType.Wall => [CellType.Wall, CellType.Wall],
                    CellType.Box => [CellType.BoxLeft, CellType.BoxRight],
                    CellType.Empty => [CellType.Empty, CellType.Empty],
                    CellType.Robot => [CellType.Robot, CellType.Empty],
                    _ => throw new ArgumentOutOfRangeException(nameof(grid))
                };

                newRow.AddRange(newItems);
            }

            newGrid.Add(newRow);
        }

        return newGrid;
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
        else if (nextItem is CellType.Box || 
            ((nextItem is CellType.BoxLeft || nextItem is CellType.BoxRight) &&
            (direction is Direction.Left || direction is Direction.Right)))
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
                else if (nextItem is CellType.Box || nextItem is CellType.BoxLeft || nextItem is CellType.BoxRight)
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
        else if (nextItem is CellType.BoxLeft || nextItem is CellType.BoxRight)
        {
            var (otherSideBox, _) = grid.GetAdjacentItem(nextLocation, nextItem is CellType.BoxLeft ? Direction.Right : Direction.Left);
            
            Stack<(Point one, Point two)> items = [];
            items.Push((currentPosition, nextLocation));

            List<Point> boxes = [nextLocation, otherSideBox];
            List<Point> newBoxes = [];
            while (boxes.Count is not 0)
            {
                foreach (var box in boxes)
                {
                    (nextLocation, nextItem) = grid.GetAdjacentItem(box, direction);
                    if (nextItem is CellType.Wall)
                    {
                        return;
                    }
                    else if (nextItem is CellType.Empty)
                    {
                        items.Push((box, nextLocation));
                        continue;
                    }
                    else if (nextItem is CellType.BoxLeft || nextItem is CellType.BoxRight)
                    {
                        (otherSideBox, _) = grid.GetAdjacentItem(nextLocation, nextItem is CellType.BoxLeft ? Direction.Right : Direction.Left);
                        items.Push((box, nextLocation));
                        newBoxes.AddRange([nextLocation, otherSideBox]);
                    }
                }

                boxes = newBoxes.Distinct().ToList();
                newBoxes = [];
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