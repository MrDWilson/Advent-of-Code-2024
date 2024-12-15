using System.Drawing;

namespace AdventOfCode.Helpers;

public enum Direction { Up, Down, Left, Right }

public class Grid<T>(List<List<T>> _data) where T : struct
{
    public T? this[Point point]
    {
        get
        {
            if (OutOfBounds(point)) return null;
            return _data[point.X][point.Y];
        }

        set 
        {
            if (OutOfBounds(point)) throw new  ArgumentOutOfRangeException(nameof(point));
            if (value is null) throw new ArgumentNullException(nameof(value));

            _data[point.X][point.Y] = value.Value;
        }
    }

    public IEnumerable<T> GetUniqueItems()
    {
        return _data.SelectMany(x => x).Distinct();
    }

    public IEnumerable<Point> FindItems(T item)
    {
        return _data.Index()
            .SelectMany(x => x.Item.Index().Select(y => new Point(x.Index, y.Index)))
            .Where(x => this[x] is T t && t.Equals(item));
    }

    public IEnumerable<Point> FindConnectedRegion(Point start)
    {
        T targetValue = this[start] ?? throw new ArgumentOutOfRangeException(nameof(start));

        List<Point> visited = [];

        var directions = new Point[] 
        {
            new(-1, 0), // up
            new(1, 0),  // down
            new(0, -1), // left
            new(0, 1)   // right
        };

        var queue = new Queue<Point>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var point = queue.Dequeue();
            yield return point;

            var neighbors = directions
                .Select(d => new Point(point.X + d.X, point.Y + d.Y))
                .Where(n => !OutOfBounds(n));

            foreach (var neighbour in neighbors)
            {
                if (!visited.Contains(neighbour) && this[neighbour].Equals(targetValue))
                {
                    visited.Add(neighbour);
                    queue.Enqueue(neighbour);
                }
            }
        }
    }

    public IEnumerable<Point> CalculateRegionPerimeter(IEnumerable<Point> points)
    {
        var pointsSet = new HashSet<Point>(points);

        var directions = new Point[] 
        {
            new(-1, 0), // up
            new(1, 0),  // down
            new(0, -1), // left
            new(0, 1)   // right
        };

        foreach (var point in pointsSet)
        {
            foreach (var direction in directions)
            {
                var neighbour = new Point(point.X + direction.X, point.Y + direction.Y);
                if (OutOfBounds(neighbour) || !pointsSet.Contains(neighbour))
                {
                    yield return neighbour;
                }
            }
        }
    }

    public void SwapItems(Point point1, Point point2)
    {
        (_data[point1.X][point1.Y], _data[point2.X][point2.Y]) = (_data[point2.X][point2.Y], _data[point1.X][point1.Y]);
    }

    public (Point, T?) GetAdjacentItem(Point point, Direction direction)
    {
        Point location = direction switch
        {
            Direction.Up => new(point.X - 1, point.Y), 
            Direction.Down => new(point.X + 1, point.Y), 
            Direction.Left => new(point.X, point.Y - 1), 
            Direction.Right => new(point.X, point.Y + 1),
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };

        return (location, this[location]);
    }

    public IEnumerable<Point> GetSurroundingItems(Point point)
    {
        Point[] locations = 
        [
            new Point(point.X - 1, point.Y), 
            new Point(point.X + 1, point.Y), 
            new Point(point.X, point.Y - 1), 
            new Point(point.X, point.Y + 1)
        ];

        return locations.Where(x => !OutOfBounds(x));
    }

    private bool OutOfBounds(Point coords)
        => coords.X < 0 || coords.Y < 0 || coords.X >= _data.Count || coords.Y >= _data.First().Count;
}