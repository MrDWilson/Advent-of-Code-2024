using System.Drawing;
using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day14(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 14;

    public record LongPoint(long Y, long X);
    public record Robot(LongPoint Position, LongPoint Velocity);
    public async Task<long> Solve()
    {
        static LongPoint ParsePoint(string s)
        {
            var raw = s.Split("=")[1];
            var points = raw.Split(",");
            return new LongPoint(long.Parse(points[0]), long.Parse(points[1]));
        }

        var data = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var robots = data.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => 
            {
                var points = x.Split(" ");
                return new Robot(ParsePoint(points[0]), ParsePoint(points[1]));
            });

        var bathroomSize = options.Value.RunType is RunType.Test ? new LongPoint(10, 6) : new LongPoint(100, 102);
        var secondsToCheck = 100;

        if (options.Value.SolutionType is SolutionType.First)
        {
            foreach (var _ in Enumerable.Range(1, secondsToCheck))
            {
                robots = SimulateSecond(robots, bathroomSize).ToList();
            }

            var quadTypes = Enum.GetValues<Quadrant>();
            var quads = quadTypes.Select(x => GetQuadrant(bathroomSize, x));
            return quads.Select(x => GetCountInQuadrant(robots, x.Y, x.X)).Aggregate((x, y) => x * y);
        }
        else
        {
            int seconds = 1;
            while(true)
            {
                robots = SimulateSecond(robots, bathroomSize).ToList();
                if (AnyHighConnectedRegion(robots.Select(x => x.Position), bathroomSize)) return seconds;
                seconds++;
            }
        }
    }

    private static bool AnyHighConnectedRegion(IEnumerable<LongPoint> positions, LongPoint bounds)
    {
        // Brute force rather than having to reivew thousands of images
        var rows = Enumerable.Repeat(0, (int)bounds.X + 1)
            .Select(_ => Enumerable.Repeat(false, (int)bounds.Y + 1).ToList())
            .ToList();

        foreach (var position in positions)
        {
            rows[(int)position.X][(int)position.Y] = true;
        }

        Grid<bool> grid = new(rows);

        foreach (var position in positions)
        {
            var connectedRegion = grid.FindConnectedRegion(new Point((int)position.X, (int)position.Y));

            // If we've got more than 50 connected robots, assume this is a picture
            if (connectedRegion.Count() > 50)
            {
                return true;
            }
        }

        return false;
    }

    private static int GetCountInQuadrant(IEnumerable<Robot> robots, Range y, Range x)
    {
        bool MatchX(long X) => X >= x.Start.Value && X <= x.End.Value;
        bool MatchY(long Y) => Y >= y.Start.Value && Y <= y.End.Value;
        return robots.Count(r => MatchX(r.Position.X) && MatchY(r.Position.Y));
    }

    private enum Quadrant { TopLeft, TopRight, BottomLeft, BottomRight }
    private static (Range Y, Range X) GetQuadrant(LongPoint bounds, Quadrant quadrant)
    {
        LongPoint quadStart = quadrant switch
        {
            Quadrant.TopLeft => new(0, 0),
            Quadrant.TopRight => new(0, (bounds.Y / 2) + 1),
            Quadrant.BottomLeft => new((bounds.X / 2) + 1, 0),
            Quadrant.BottomRight => new((bounds.X / 2) + 1, (bounds.Y / 2) + 1),
            _ => throw new ArgumentOutOfRangeException(nameof(quadrant))
        };

        LongPoint quadEnd = quadrant switch
        {
            Quadrant.TopLeft => new((bounds.X / 2) - 1, (bounds.Y / 2) - 1),
            Quadrant.TopRight => new((bounds.X / 2) - 1, bounds.Y),
            Quadrant.BottomLeft => new(bounds.X, (bounds.Y / 2) - 1),
            Quadrant.BottomRight => new(bounds.X, bounds.Y),
            _ => throw new ArgumentOutOfRangeException(nameof(quadrant))
        };

        return ((int)quadStart.X..(int)quadEnd.X, (int)quadStart.Y..(int)quadEnd.Y);
    }

    private static IEnumerable<Robot> SimulateSecond(IEnumerable<Robot> robots, LongPoint bounds)
    {
        foreach (var robot in robots)
        {
            yield return new Robot(MovePoint(robot, bounds), robot.Velocity);
        }
    }

    private static LongPoint MovePoint(Robot robot, LongPoint bounds)
    {
        var newX = robot.Position.X + robot.Velocity.X;
        var newY = robot.Position.Y + robot.Velocity.Y;

        if (newX > bounds.X)
        {
            newX = -1 + (newX - bounds.X);
        }
        else if (newX < 0)
        {
            newX = bounds.X - (-1 - newX);
        }

        if (newY > bounds.Y)
        {
            newY = -1 + (newY - bounds.Y);
        }
        else if (newY < 0)
        {
            newY = bounds.Y - (-1 - newY);
        }

        return new LongPoint(newY, newX);
    }
}