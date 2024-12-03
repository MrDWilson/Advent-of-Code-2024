using System.Text.RegularExpressions;
using AdventOfCode.Models;
using AdventOfCode.Solutions.Base;

namespace AdventOfCode.Solutions;

public partial class Day3 : SolutionBase
{
    public override int Day => 3;

    [GeneratedRegex(@"mul\((\d+,\d+)\)")]
    private static partial Regex Exp1();

    [GeneratedRegex(@"((mul|do|don\'t))\((\d+,\d+)?\)")]
    private static partial Regex Exp2();

    public override void Solve(SolutionType type, string[] content)
    {
        var instructions = string.Join("", content);

        if (type is SolutionType.First)
        {
            var matches = Exp1().Matches(instructions);
            var numberPairs = matches.Select(x => x.Groups[1])
                .Select(x => x.Value.Split(","))
                .Select(x => (int.Parse(x.First()), int.Parse(x.Last())));
            Console.WriteLine(numberPairs.Sum(x => x.Item1 * x.Item2));
        }
        else
        {
            var matches = Exp2().Matches(instructions);

            List<(int, int)> results = [];
            bool enabled = true;
            foreach (var match in matches.Cast<Match>())
            {
                switch (match.Groups[1].Value)
                {
                    case "mul":
                        if (enabled)
                        {
                            var numbers = match.Groups[3].Value.Split(",");
                            results.Add((int.Parse(numbers.First()), int.Parse(numbers.Last())));
                        }
                        break;
                    case "do":
                        enabled = true;
                        break;
                    case "don't":
                        enabled = false;
                        break;
                }
            }
            
            Console.WriteLine(results.Sum(x => x.Item1 * x.Item2));
        }
    }
}