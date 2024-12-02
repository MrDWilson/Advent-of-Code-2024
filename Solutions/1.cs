using AdventOfCode.Models;

namespace AdventOfCode.Solutions;

public class Day1 : SolutionBase
{
    public override int Day => 1;

    public override void Solve(SolutionType type, string[] content)
    {
        var lines = ToIntArray(content);
        var listOne = lines.Select(parts => parts.First()).ToList();
        var listTwo = lines.Select(parts => parts.Last()).ToList();
        var result = type switch
        {
            SolutionType.First => listOne.Order().Zip(listTwo.Order(), (x, y) => Math.Abs(x - y)).Sum(),
            SolutionType.Second => listOne.Sum(x => x * listTwo.Count(y => y == x)),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        Console.WriteLine(result);
    }
}