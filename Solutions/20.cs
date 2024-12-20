using System.Collections.Concurrent;
using System.Drawing;
using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day20(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 20;

    private enum CellType { Wall, Empty, Start, End }
    private record MazeEntry(Point Location, Direction Direction, int Steps);

    private Point Start;
    private Point End;
    private int? MainCost;

    public async Task<string> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var rows = lines.Select(x => x.Select(ParseCell).ToList()).ToList();
        var Grid = new Grid<CellType>(rows);
        Start = Grid.FindItems(CellType.Start).Single();
        End = Grid.FindItems(CellType.End).Single();

        if (options.Value.SolutionType is SolutionType.First)
        {
            (MainCost, _) = SolveMaze(Grid);

            var walls = Grid.FindItems(CellType.Wall)
                .Where(x => Grid.GetSurroundingItems(x).Count(y => Grid[y] is not CellType.Wall) > 1)
                .ToList();

            ConcurrentBag<int> countCheats = [];
            Parallel.ForEach(walls.Chunk(100), wallChunk => 
            {
                foreach (var wall in wallChunk)
                {
                    var gridCopy = Grid.Copy();
                    gridCopy[wall] = CellType.Empty;

                    var (solution, _) = SolveMaze(gridCopy);
                    if (solution is not null) countCheats.Add(1);
                }
            });

            return countCheats.Count.ToString();
        }
        else
        {
            (MainCost, var route) = SolveMaze(Grid);
            if (route is null) return "0";

            HashSet<(Point one, Point two)> potentialCheats = [];
            var routeIndexed = route.Index().ToList();
            foreach (var point in routeIndexed)
            {
                foreach (var other in routeIndexed)
                {
                    if (other.Index <= point.Index) continue;
                    if (point.Item == other.Item) continue;

                    var routeDifference = Math.Abs(other.Index - point.Index);
                    var manhattanDistance = Math.Abs(point.Item.X - other.Item.X) + Math.Abs(point.Item.Y - other.Item.Y);
                    if (routeDifference - manhattanDistance >= (options.Value.RunType is RunType.Test ? 50 : 100) && manhattanDistance <= 20)
                    {
                        var shortcut = point.Item.X <= other.Item.X ? (point.Item, other.Item) : (other.Item, point.Item);
                        potentialCheats.Add(shortcut);
                    }
                }
            }

            return potentialCheats.Count.ToString();
        }
    }

    private static CellType ParseCell(char c)
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

    private (int?, List<Point>?) SolveMaze(Grid<CellType> grid)
    {
        var startPosition = Start;
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
            if (currentCost + 100 > MainCost) return (null, null);

            if (visitedStates.TryGetValue((current.Location, current.Direction), out var knownCost) && knownCost < currentCost)
                continue;

            var (nextPosition, nextItem) = grid.GetAdjacentItem(current.Location, current.Direction);
            if (nextPosition == End)
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
                if (grid.GetAdjacentItem(current.Location, nextDirection).Item2 is not null)
                {
                    var rotateSteps = current.Steps;
                    var rotateCost = rotateSteps;
                    TryEnqueueState(current, current.Location, nextDirection, rotateSteps, rotateCost, parents, visitedStates, pq);
                }
            }
        }

        if (!bestCost.HasValue)
            return (null, null);

        return (bestCost.Value, GetPath(parents, End, endStates.First().Direction));
    }

    private List<Point> GetPath(Dictionary<(Point, Direction), (int cost, List<(Point, Direction, int)>)> parents, Point end, Direction endDirection)
    {
        var path = new List<Point>();
        var currentKey = (end, endDirection);

        while (currentKey.end != Start)
        {
            if (!parents.ContainsKey(currentKey))
            {
                throw new InvalidOperationException("Path reconstruction failed. Missing parent state.");
            }

            path.Add(currentKey.end);

            var parentState = parents[currentKey].Item2.First();
            currentKey = (parentState.Item1, parentState.Item2);
        }

        path.Add(Start);

        path.Reverse();

        return path.Distinct().ToList();
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
}