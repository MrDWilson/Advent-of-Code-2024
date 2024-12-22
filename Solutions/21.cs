using System.Drawing;
using System.Reflection.Emit;
using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day21(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 21;

    public enum Actions { Up, Down, Left, Right, Press }
    public async Task<string> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var codes = lines.Select(x => x.ToCharArray()).ToList();

        List<List<char>> keypadLines =
        [
            ['7', '8', '9'],
            ['4', '5', '6'],
            ['1', '2', '3'],
            [' ', '0', 'A']
        ];
        var keypad = new Grid<char>(keypadLines);

        List<List<char>> arrowLines =
        [
            [' ', '^', 'A'],
            ['<', 'v', '>']
        ];
        var arrows = new Grid<char>(arrowLines);
        var arrowStartingPoint = arrows.FindItems('A').Single();

        var keypadRobot = keypad.FindItems('A').Single();
        var arrowRobot1 = arrowStartingPoint;
        var arrowRobot2 = arrowStartingPoint;

        long total = 0;
        foreach (var code in codes)
        {
            List<Actions> humanSteps = [];
            foreach (var c in code)
            {
                List<Actions> directionOne = [], directionTwo = [];
                Point keypadRobotCache = keypadRobot, arrowRobot1Cache = arrowRobot1, arrowRobot2Cache = arrowRobot2;
                foreach (var direction in new bool[] { true, false })
                {
                    if (!direction)
                    {
                        keypadRobot = keypadRobotCache;
                        arrowRobot1 = arrowRobot1Cache;
                        arrowRobot2 = arrowRobot2Cache;
                    }

                    (var steps, keypadRobot) = CalculateSteps(keypad, c, keypadRobot, direction);

                    steps = steps.SelectMany(x => 
                    {
                        (var result, arrowRobot1) = CalculateSteps(arrows, ActionToChar(x), arrowRobot1, direction);
                        return result;
                    }).ToList();
                    
                    steps = steps.SelectMany(x => 
                    {
                        (var result, arrowRobot2) = CalculateSteps(arrows, ActionToChar(x), arrowRobot2, direction);
                        return result;
                    }).ToList();

                    if (direction)
                        directionOne.AddRange(steps);
                    else
                        directionTwo.AddRange(steps);
                }

                if (directionOne.Count > directionTwo.Count)
                    humanSteps.AddRange(directionTwo);
                else if (directionOne.Count < directionTwo.Count)
                    humanSteps.AddRange(directionOne);
                else
                {
                    var directionOneDistance = directionOne.GroupWhile((a, b) => a == b).Where(x => x.Count() > 1).Count();
                    var directionTwoDistance = directionTwo.GroupWhile((a, b) => a == b).Where(x => x.Count() > 1).Count();

                    humanSteps.AddRange(directionOneDistance > directionTwoDistance ? directionOne : directionTwo);
                }
            }
            
            Console.WriteLine(string.Join("", humanSteps.Select(ActionToChar)));
            Console.WriteLine(humanSteps.Count);
            var digit = int.Parse(new string(code.Where(x => x is >= '0' and <= '9').ToArray()));
            total += digit * humanSteps.Count;
        }

        return total.ToString();
    }

    private static readonly Dictionary<(char, Point), List<Actions>> Memo = [];
    private static (List<Actions>, Point) CalculateSteps(Grid<char> grid, char c, Point startPoint, bool yFirst)
    {
        var destination = grid.FindItems(c).Single();
        var arrowPad = !grid.FindItems('0').Any();
        if (arrowPad)
        {
            if (Memo.TryGetValue((c, startPoint), out var result)) return (result, destination);
        }

        var xSteps = destination.X - startPoint.X;
        var ySteps = destination.Y - startPoint.Y;
        var xDirection = xSteps > 0 ? Direction.Down : Direction.Up;
        var yDirection = ySteps > 0 ? Direction.Right : Direction.Left;

        var steps = GetSteps();

        if (arrowPad && (!Memo.ContainsKey((c, startPoint)) || (Memo[(c, startPoint)].Count > steps.Count)))
        {
            Memo[(c, startPoint)] = steps;
        }

        return (steps, destination);

        List<Actions> GetSteps()
        {
            var xStepCount = Math.Abs(xSteps);
            var yStepCount = Math.Abs(ySteps);
            var currentPoint = startPoint;
            var steps = new List<Actions>();
            while (xStepCount + yStepCount > 0)
            {
                bool DoY()
                {
                    if (yStepCount > 0)
                    {
                        Point nextPoint;
                        if (yDirection is Direction.Right)
                            nextPoint = new(currentPoint.X, currentPoint.Y + 1);
                        else
                            nextPoint = new(currentPoint.X, currentPoint.Y - 1);

                        if (grid[nextPoint] is not ' ')
                        {
                            currentPoint = nextPoint;
                            steps.Add(yDirection == Direction.Right ? Actions.Right : Actions.Left);
                            yStepCount--;
                            return true;
                        }
                    }

                    return false;
                }

                bool DoX()
                {
                    if (xStepCount > 0)
                    {
                        Point nextPoint;
                        if (xDirection is Direction.Down)
                            nextPoint = new(currentPoint.X + 1, currentPoint.Y);
                        else
                            nextPoint = new(currentPoint.X - 1, currentPoint.Y);

                        if (grid[nextPoint] is not ' ')
                        {
                            currentPoint = nextPoint;
                            steps.Add(xDirection == Direction.Down ? Actions.Down : Actions.Up);
                            xStepCount--;
                            return true;
                        }
                    }

                    return false;
                }

                if (yFirst)
                {
                    if (!DoY())
                        DoX();
                }
                else
                {
                    if (!DoX())
                        DoY();
                }
            }
            steps.Add(Actions.Press);

            return steps;
        }
    }

    private static char ActionToChar(Actions a) => a switch
    {
        Actions.Up => '^',
        Actions.Down => 'v',
        Actions.Left => '<',
        Actions.Right => '>',
        Actions.Press => 'A',
        _ => throw new ArgumentOutOfRangeException(nameof(a))
    };
}