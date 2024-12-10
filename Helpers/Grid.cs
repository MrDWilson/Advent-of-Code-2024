using System.Drawing;

namespace AdventOfCode.Helpers;

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

    public IEnumerable<Point> FindItems(T item)
    {
        return _data.Index()
            .SelectMany(x => x.Item.Index().Select(y => new Point(x.Index, y.Index)))
            .Where(x => this[x] is T t && t.Equals(item));
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