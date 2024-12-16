using System.Drawing;
using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day16(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 16;

    private enum CellType { Wall, Empty, Start, End }
    private record MazeEntry(Point Location, Direction Direction, int Rotations, int Steps);

    private Grid<CellType> Grid = null!;

    public async Task<long> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var gridLines = lines.Select(x => x.Select(GetCell).ToList()).ToList();
        Grid = new Grid<CellType>(gridLines);

        var startPosition = Grid.FindItems(CellType.Start).Single();
        var startDirection = Direction.Right;

        var pq = new PriorityQueue<MazeEntry, int>();
        var visitedStates = new Dictionary<(Point, Direction), int>();

        var parents = new Dictionary<(Point, Direction), (int cost, List<(Point, Direction, int, int)>)>();

        var startEntry = new MazeEntry(startPosition, startDirection, 0, 0);
        var startCost = 0;
        pq.Enqueue(startEntry, startCost);
        visitedStates[(startPosition, startDirection)] = startCost;
        parents[(startPosition, startDirection)] = (startCost, new List<(Point, Direction, int, int)>()); // start has no parents

        int? bestCost = null;
        var endStates = new List<(Point Location, Direction Direction, int Rotations, int Steps)>();

        while (pq.TryDequeue(out var current, out var currentCost))
        {
            if (visitedStates.TryGetValue((current.Location, current.Direction), out var knownCost) && knownCost < currentCost)
                continue;

            var (nextPosition, nextItem) = Grid.GetAdjacentItem(current.Location, current.Direction);
            var currentTotalCost = current.Steps + current.Rotations * 1000;

            if (nextItem is CellType.End)
            {
                var finalCost = current.Steps + 1 + current.Rotations * 1000;
                if (bestCost is null || finalCost < bestCost)
                {
                    bestCost = finalCost;
                    endStates.Clear();
                    endStates.Add(new(nextPosition, current.Direction, current.Rotations, current.Steps + 1));

                    var endKey = (nextPosition, current.Direction);
                    if (!parents.ContainsKey(endKey))
                    {
                        parents[endKey] = (finalCost, new List<(Point, Direction, int, int)> 
                        { (current.Location, current.Direction, current.Rotations, current.Steps) });
                    }
                    else if (parents[endKey].cost == finalCost)
                    {
                        parents[endKey].Item2.Add((current.Location, current.Direction, current.Rotations, current.Steps));
                    }

                }
                else if (bestCost == finalCost)
                {
                    endStates.Add(new(nextPosition, current.Direction, current.Rotations, current.Steps + 1));
                    var endKey = (nextPosition, current.Direction);
                    if (!parents.ContainsKey(endKey))
                    {
                        parents[endKey] = (finalCost, new List<(Point, Direction, int, int)>
                        { (current.Location, current.Direction, current.Rotations, current.Steps) });
                    }
                    else
                    {
                        parents[endKey].Item2.Add((current.Location, current.Direction, current.Rotations, current.Steps));
                    }
                }
                continue;
            }

            if (bestCost.HasValue && currentCost >= bestCost) continue;

            if (nextItem is CellType.Empty)
            {
                var forwardSteps = current.Steps + 1;
                var forwardRotations = current.Rotations;
                var forwardCost = forwardSteps + forwardRotations * 1000;
                TryEnqueueState(current, nextPosition, current.Direction, forwardRotations, forwardSteps, forwardCost, parents, visitedStates, pq);
            }

            foreach (var nextDirection in GetNextDirections(current.Direction))
            {
                if (Grid.GetAdjacentItem(current.Location, nextDirection).Item2 != CellType.Wall)
                {
                    var rotateSteps = current.Steps;
                    var rotateRotations = current.Rotations + 1;
                    var rotateCost = rotateSteps + rotateRotations * 1000;
                    TryEnqueueState(current, current.Location, nextDirection, rotateRotations, rotateSteps, rotateCost, parents, visitedStates, pq);
                }
            }
        }

        if (!bestCost.HasValue)
            throw new Exception("No route found to the end.");

        var allRoutes = new List<List<(Point, Direction)>>();
        foreach (var endState in endStates)
        {
            var route = new List<(Point, Direction)>();
            BacktrackRoute((endState.Location, endState.Direction), parents, route, allRoutes);
        }

        return options.Value.SolutionType is SolutionType.First ? bestCost.Value : allRoutes.SelectMany(x => x.Select(y => y.Item1)).Distinct().Count();
    }

    private static void TryEnqueueState(
        MazeEntry parentState,
        Point nextPos,
        Direction nextDir,
        int nextRotations,
        int nextSteps,
        int newCost,
        Dictionary<(Point, Direction), (int cost, List<(Point, Direction, int, int)>)> parents,
        Dictionary<(Point, Direction), int> visitedStates,
        PriorityQueue<MazeEntry,int> pq)
    {
        var key = (nextPos, nextDir);

        if (!visitedStates.TryGetValue(key, out var knownCost))
        {
            visitedStates[key] = newCost;
            parents[key] = (newCost, new List<(Point, Direction, int, int)>
            {
                (parentState.Location, parentState.Direction, parentState.Rotations, parentState.Steps)
            });
            pq.Enqueue(new MazeEntry(nextPos, nextDir, nextRotations, nextSteps), newCost);
        }
        else if (knownCost == newCost)
        {
            parents[key].Item2.Add((parentState.Location, parentState.Direction, parentState.Rotations, parentState.Steps));
        }
        else if (knownCost > newCost)
        {
            visitedStates[key] = newCost;
            parents[key] = (newCost, new List<(Point, Direction, int, int)>
            {
                (parentState.Location, parentState.Direction, parentState.Rotations, parentState.Steps)
            });
            pq.Enqueue(new MazeEntry(nextPos, nextDir, nextRotations, nextSteps), newCost);
        }
    }

    private void BacktrackRoute(
        (Point Location, Direction Direction) state,
        Dictionary<(Point, Direction), (int cost, List<(Point, Direction, int, int)>)> parents,
        List<(Point, Direction)> currentRoute,
        List<List<(Point, Direction)>> allRoutes)
    {
        currentRoute.Insert(0, state);

        if (parents[state].Item2.Count is 0 && Grid[state.Location] is CellType.Start)
        {
            allRoutes.Add([.. currentRoute]);
            currentRoute.RemoveAt(0);
            return;
        }

        foreach (var parent in parents[state].Item2)
        {
            BacktrackRoute((parent.Item1, parent.Item2), parents, currentRoute, allRoutes);
        }

        currentRoute.RemoveAt(0);
    }

    private static IEnumerable<Direction> GetNextDirections(Direction direction)
    {
        return direction switch
        {
            Direction.Up => [Direction.Left, Direction.Right],
            Direction.Down => [Direction.Left, Direction.Right],
            Direction.Left => [Direction.Up, Direction.Down],
            Direction.Right => [Direction.Up, Direction.Down],
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
    }

    private static CellType GetCell(char c)
    {
        return c switch
        {
            '#' => CellType.Wall,
            '.' => CellType.Empty,
            'S' => CellType.Start,
            'E' => CellType.End,
            _ => throw new ArgumentOutOfRangeException(nameof(c))
        };
    }
}