using System.Text.RegularExpressions;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day3(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 3;

    [GeneratedRegex(@"mul\((\d+),(\d+)\)")]
    private static partial Regex Exp1();

    [GeneratedRegex(@"(mul|do|don't)\((?:(\d+),(\d+))?\)")]
    private static partial Regex Exp2();

    public async Task<string> Solve()
    {
        var instructions = await loader.LoadRaw(Day, options.Value.SolutionType, options.Value.RunType);

        int sum = 0;
        if (options.Value.SolutionType is SolutionType.First)
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

        return sum.ToString();
    }
}