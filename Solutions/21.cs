using System.Collections.Immutable;
using System.Drawing;
using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

class ImmutableArrayComparer<T> : IEqualityComparer<(ImmutableArray<T>, int)>
{
    public bool Equals((ImmutableArray<T>, int) x, (ImmutableArray<T>, int) y)
    {
        return x.Item1.SequenceEqual(y.Item1) && x.Item2 == y.Item2;
    }

    public int GetHashCode((ImmutableArray<T>, int) obj)
    {
        int hashcode = 0;
        foreach (T t in obj.Item1)
        {
            hashcode ^= t!.GetHashCode();
        }
        return hashcode ^= obj.Item2;
    }
}

public partial class Day21(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 21;

    public enum Actions { Up, Down, Left, Right, Press }
    public async Task<string> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var codes = lines.Select(x => x.ToCharArray()).ToList();

        List<List<char>> keypadLines = [
            ['7', '8', '9'],
            ['4', '5', '6'],
            ['1', '2', '3'],
            [' ', '0', 'A']
        ];
        var keypad = new Grid<char>(keypadLines);

        List<List<char>> arrowLines = [
            [' ', '^', 'A'],
            ['<', 'v', '>']
        ];
        var arrows = new Grid<char>(arrowLines);

        var keypadRobot = keypad.FindItems('A').Single();
        var arrowStart  = arrows.FindItems('A').Single();

        int depth = options.Value.SolutionType is SolutionType.First ? 2 : 25;
        long total = 0;
        foreach (var code in codes)
        {
            long bestCostForThisCode = long.MaxValue;
            var digit = int.Parse(new string(code.Where(char.IsDigit).ToArray()));

            var allKeypadSequences = GetAllKeypadSequences(keypad, keypadRobot, code);
            foreach (var keyPadPath in allKeypadSequences)
            {
                long cost = CountPushes(keyPadPath, depth, arrows);
                if (cost < bestCostForThisCode)
                {
                    bestCostForThisCode = cost;
                }
            }

            total += bestCostForThisCode * digit;
        }

        return total.ToString();
    }

    private static List<List<Actions>> GetAllKeypadSequences(Grid<char> keypad, Point start, char[] code)
    {
        return RecurseAllKeypadPaths(keypad, start, code, 0);
    }

    private static List<List<Actions>> RecurseAllKeypadPaths(Grid<char> keypad, Point currentPos, char[] code, int index)
    {
        if (index == code.Length)
        {
            return [[]];
        }

        var nextChar = code[index];
        var allPaths = BfsAllShortestPaths(keypad, currentPos, nextChar);

        if (allPaths.Count is 0)
        {
            return [];
        }

        var result = new List<List<Actions>>();
        foreach (var path in allPaths)
        {
            var endPos = ComputeEndPoint(currentPos, path);
            var tails = RecurseAllKeypadPaths(keypad, endPos, code, index + 1);
            foreach (var t in tails)
            {
                var combined = new List<Actions>(path);
                combined.AddRange(t);
                result.Add(combined);
            }
        }

        return result;
    }

    private static Point ComputeEndPoint(Point start, List<Actions> path)
    {
        var end = start;
        foreach (var i in Enumerable.Range(0, path.Count))
        {
            if (path[i] is Actions.Press) break;
            var (dx, dy) = Directions[path[i]];
            end = new Point(end.X + dx, end.Y + dy);
        }
        return end;
    }

    private static readonly Dictionary<(ImmutableArray<Actions>, int), long> PushesMemo = new(new ImmutableArrayComparer<Actions>());
    private static long CountPushes(List<Actions> keyPadPath, int depth, Grid<char> arrowPad)
    {
        var cacheKey = (keyPadPath.ToImmutableArray(), depth);
        if (PushesMemo.TryGetValue(cacheKey, out var memo)) return memo;

        if (depth is 0)
        {
            return PushesMemo[cacheKey] = keyPadPath.Count;
        }

        var position = arrowPad.FindItems('A').Single();
        long total = 0;
        foreach (var action in keyPadPath)
        {
            char arrowSymbol = ActionToChar(action);
            var roads = BfsAllShortestPaths(arrowPad, position, arrowSymbol);
            if (roads.Count is 0)
            {
                return PushesMemo[cacheKey] = long.MaxValue;
            }

            long minCost = long.MaxValue;
            foreach (var road in roads)
            {
                long costOfThisRoad = CountPushesRoad(road, depth - 1, arrowPad);
                if (costOfThisRoad < minCost) 
                    minCost = costOfThisRoad;
            }

            total += minCost;

            position = arrowPad.FindItems(arrowSymbol).First();
        }

        return PushesMemo[cacheKey] = total;
    }

    private static readonly Dictionary<(ImmutableArray<Actions>, int), long> RoadMemo = new(new ImmutableArrayComparer<Actions>());
    private static long CountPushesRoad(List<Actions> arrowPath, int depth, Grid<char> arrowPad)
    {
        var cacheKey = (arrowPath.ToImmutableArray(), depth);
        if (RoadMemo.TryGetValue(cacheKey, out var memoValue))return memoValue;

        if (depth is 0)
        {
            return RoadMemo[cacheKey] = arrowPath.Count;
        }

        var position = arrowPad.FindItems('A').Single();
        long sum = 0;
        foreach (var action in arrowPath)
        {
            char c = ActionToChar(action);
            var roads = BfsAllShortestPaths(arrowPad, position, c);
            if (roads.Count is 0)
            {
                return RoadMemo[cacheKey] = long.MaxValue;
            }

            long min = long.MaxValue;
            foreach (var r in roads)
            {
                long cost = CountPushesRoad(r, depth - 1, arrowPad);
                if (cost < min) min = cost;
            }

            sum += min;
            position = arrowPad.FindItems(c).First();
        }

        return RoadMemo[cacheKey] = sum;
    }

    private static readonly Dictionary<Actions, (int dx, int dy)> Directions = new()
    {
        [Actions.Up]    = (-1, 0),
        [Actions.Down]  = ( 1, 0),
        [Actions.Left]  = ( 0,-1),
        [Actions.Right] = ( 0, 1),
    };
    private static List<List<Actions>> BfsAllShortestPaths(
        Grid<char> grid,
        Point start,
        char goal
    )
    {
        if (grid[start] == goal)
        {
            return [[Actions.Press]];
        }

        var queue = new Queue<(Point Current, List<Actions> Path)>();
        queue.Enqueue((start, new List<Actions>()));

        int bestDistance = int.MaxValue;
        var shortestPaths = new List<List<Actions>>();
        var distMap = new Dictionary<Point, int> { [start] = 0 };
        while (queue.Count > 0)
        {
            var (current, pathSoFar) = queue.Dequeue();
            int currentDistance = pathSoFar.Count;

            if (currentDistance > bestDistance) 
                continue;

            foreach (var (action, (dx, dy)) in Directions)
            {
                var nextPoint = new Point(current.X + dx, current.Y + dy);
                if (grid.OutOfBounds(nextPoint) || grid[nextPoint] is ' ') 
                    continue;

                var nextPath = new List<Actions>(pathSoFar) { action };
                if (!distMap.TryGetValue(nextPoint, out var oldDist) || oldDist >= nextPath.Count)
                {
                    distMap[nextPoint] = nextPath.Count;
                    
                    if (grid[nextPoint] == goal)
                    {
                        nextPath.Add(Actions.Press);
                        if (nextPath.Count < bestDistance)
                        {
                            bestDistance = nextPath.Count;
                            shortestPaths.Clear();
                        }
                        shortestPaths.Add(nextPath);
                    }
                    else
                    {
                        queue.Enqueue((nextPoint, nextPath));
                    }
                }
            }
        }

        return shortestPaths;
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