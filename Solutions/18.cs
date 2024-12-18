using System.Drawing;
using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day18(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 18;

    private enum CellType { Byte, Empty }
    private record MazeEntry(Point Location, Direction Direction, int Steps);

    private Grid<CellType> Grid = null!;
    private int Rows;
    private int Cols;

    public async Task<string> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var bytes = lines.Select(x => x.Split(",")).Select(x => (int.Parse(x[0].Trim()), int.Parse(x[1].Trim()))).ToList();

        if (options.Value.RunType is RunType.Test) Rows = Cols = 6;
        else Rows = Cols = 70;
        var gridLines = Enumerable.Repeat(CellType.Empty, Rows + 1).Select(x => Enumerable.Repeat(CellType.Empty, Cols + 1).ToList()).ToList();
        
        int byteCount = options.Value.RunType is RunType.Test ? 12 : 1024;
        foreach (var b in bytes.Take(byteCount))
        {
            gridLines[b.Item2][b.Item1] = CellType.Byte;
        }

        Grid = new Grid<CellType>(gridLines);

        if (options.Value.SolutionType is SolutionType.First)
        {
            return SolveMaze()?.ToString() ?? throw new InvalidOperationException("No route found");
        }
        else
        {
            foreach (var b in bytes.Skip(byteCount))
            {
                Grid[new Point(b.Item2, b.Item1)] = CellType.Byte;
                var solved = SolveMaze();
                if (solved is null)
                    return b.ToString();
            }

            return "No solution";
        }
    }

    private int? SolveMaze()
    {
        var startPosition = new Point(0, 0);
        var startDirection = Direction.Down;
        var pq = new PriorityQueue<MazeEntry, int>();
        var visitedStates = new Dictionary<(Point, Direction), int>();

        var parents = new Dictionary<(Point, Direction), (int cost, List<(Point, Direction, int)>)>();

        var startEntry = new MazeEntry(startPosition, startDirection, 0);
        var startCost = 0;
        pq.Enqueue(startEntry, startCost);
        visitedStates[(startPosition, startDirection)] = startCost;
        parents[(startPosition, startDirection)] = (startCost, new List<(Point, Direction, int)>()); // start has no parents

        int? bestCost = null;
        var endStates = new List<(Point Location, Direction Direction, int Steps)>();

        while (pq.TryDequeue(out var current, out var currentCost))
        {
            if (visitedStates.TryGetValue((current.Location, current.Direction), out var knownCost) && knownCost < currentCost)
                continue;

            var (nextPosition, nextItem) = Grid.GetAdjacentItem(current.Location, current.Direction);
            var currentTotalCost = current.Steps;

            if (nextPosition == new Point(Rows, Cols))
            {
                var finalCost = current.Steps + 1;
                if (bestCost is null || finalCost < bestCost)
                {
                    bestCost = finalCost;
                    endStates.Clear();
                    endStates.Add(new(nextPosition, current.Direction, current.Steps + 1));

                    var endKey = (nextPosition, current.Direction);
                    if (!parents.ContainsKey(endKey))
                    {
                        parents[endKey] = (finalCost, new List<(Point, Direction, int)> 
                        { (current.Location, current.Direction, current.Steps) });
                    }
                    else if (parents[endKey].cost == finalCost)
                    {
                        parents[endKey].Item2.Add((current.Location, current.Direction, current.Steps));
                    }

                }
                else if (bestCost == finalCost)
                {
                    endStates.Add(new(nextPosition, current.Direction, current.Steps + 1));
                    var endKey = (nextPosition, current.Direction);
                    if (!parents.ContainsKey(endKey))
                    {
                        parents[endKey] = (finalCost, new List<(Point, Direction, int)>
                        { (current.Location, current.Direction, current.Steps) });
                    }
                    else
                    {
                        parents[endKey].Item2.Add((current.Location, current.Direction, current.Steps));
                    }
                }
                continue;
            }

            if (bestCost.HasValue && currentCost >= bestCost) continue;

            if (nextItem is CellType.Empty)
            {
                var forwardSteps = current.Steps + 1;
                var forwardCost = forwardSteps;
                TryEnqueueState(current, nextPosition, current.Direction, forwardSteps, forwardCost, parents, visitedStates, pq);
            }

            foreach (var nextDirection in Enum.GetValues<Direction>())
            {
                if (Grid.GetAdjacentItem(current.Location, nextDirection).Item2 is not null)
                {
                    var rotateSteps = current.Steps;
                    var rotateCost = rotateSteps;
                    TryEnqueueState(current, current.Location, nextDirection, rotateSteps, rotateCost, parents, visitedStates, pq);
                }
            }
        }

        if (!bestCost.HasValue)
            return null;

        return bestCost.Value;
    }

    private static void TryEnqueueState(
        MazeEntry parentState,
        Point nextPos,
        Direction nextDir,
        int nextSteps,
        int newCost,
        Dictionary<(Point, Direction), (int cost, List<(Point, Direction, int)>)> parents,
        Dictionary<(Point, Direction), int> visitedStates,
        PriorityQueue<MazeEntry,int> pq)
    {
        var key = (nextPos, nextDir);

        if (!visitedStates.TryGetValue(key, out var knownCost))
        {
            visitedStates[key] = newCost;
            parents[key] = (newCost, new List<(Point, Direction, int)>
            {
                (parentState.Location, parentState.Direction, parentState.Steps)
            });
            pq.Enqueue(new MazeEntry(nextPos, nextDir, nextSteps), newCost);
        }
        else if (knownCost == newCost)
        {
            parents[key].Item2.Add((parentState.Location, parentState.Direction, parentState.Steps));
        }
        else if (knownCost > newCost)
        {
            visitedStates[key] = newCost;
            parents[key] = (newCost, new List<(Point, Direction, int)>
            {
                (parentState.Location, parentState.Direction, parentState.Steps)
            });
            pq.Enqueue(new MazeEntry(nextPos, nextDir, nextSteps), newCost);
        }
    }

    private void BacktrackRoute(
        (Point Location, Direction Direction) state,
        Dictionary<(Point, Direction), (int cost, List<(Point, Direction, int)>)> parents,
        List<(Point, Direction)> currentRoute,
        List<List<(Point, Direction)>> allRoutes)
    {
        currentRoute.Insert(0, state);

        if (parents[state].Item2.Count is 0 && state.Location == new Point(0, 0))
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
}