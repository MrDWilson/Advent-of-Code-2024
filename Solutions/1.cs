using System.Security.Cryptography.X509Certificates;
using AdventOfCode.Models;

namespace AdventOfCode.Solutions;

public class Day1 : ISolution
{
    public int Day => 1;

    public void Solve(SolutionType type, string[] content)
    {
        var lines = content.Select(x => 
        {
            var items = x.Split("   ");
            return new { First = items[0], Second = items[1] };
        });
        var listOne = lines.Select(x => int.Parse(x.First.ToString()));
        var listTwo = lines.Select(x => int.Parse(x.Second.ToString()));

        if (type is SolutionType.First)
        {
            var zipped = listOne.OrderBy(x => x).Zip(listTwo.OrderBy(x => x));

            var totalDistance = zipped.Sum(values => Math.Abs(values.First - values.Second));
            Console.WriteLine(totalDistance);
        }
        else
        {
            Console.WriteLine(listOne.Sum(x => x * listTwo.Count(y => y == x)));
        }
    }
}