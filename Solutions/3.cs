using System.Text.RegularExpressions;
using AdventOfCode.Models;
using AdventOfCode.Services;
using AdventOfCode.Solutions.Base;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day3(IFileLoader _loader, IOptions<SolutionOptions> _options) : SolutionBase(_loader, _options)
{
    public override int Day => 3;

    [GeneratedRegex(@"mul\((\d+),(\d+)\)")]
    private static partial Regex Exp1();

    [GeneratedRegex(@"(mul|do|don't)\((?:(\d+),(\d+))?\)")]
    private static partial Regex Exp2();

    public override async Task Solve()
    {
        var instructions = await Loader.LoadRaw(Day, Options.SolutionType, Options.RunType);

        int sum = 0;
        if (Options.SolutionType is SolutionType.First)
        {
            sum = Exp1().Matches(instructions)
                .Cast<Match>()
                .Sum(m => int.Parse(m.Groups[1].Value) * int.Parse(m.Groups[2].Value));
        }
        else
        {
            var enabled = true;
            foreach (Match m in Exp2().Matches(instructions))
            {
                var command = m.Groups[1].Value;
                if (command is "do")
                    enabled = true;
                else if (command is "don't")
                    enabled = false;
                else if (enabled && command is "mul")
                    sum += int.Parse(m.Groups[2].Value) * int.Parse(m.Groups[3].Value);
            }
        }

        Console.WriteLine(sum);
    }
}