using System.Drawing;
using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day13(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 13;

    public record ClawMachine(Point ButtonA, Point ButtonB, Point Prize);
    public async Task<long> Solve()
    {
        var input = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var blocks = input.Where(x => x.Trim() is { Length: > 0 }).Chunk(3);

        List<int> ProcessInput(string s, string splitChar)
        {
            return s.Split(":", StringSplitOptions.RemoveEmptyEntries)[1]
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.Parse(x.Split(splitChar)[1]))
                .ToList();
        }

        List<ClawMachine> clawMachines = [];
        foreach (var block in blocks)
        {
            var buttonA = ProcessInput(block[0], "+");
            var buttonB = ProcessInput(block[1], "+");
            var prize = ProcessInput(block[2], "=");

            clawMachines.Add(new ClawMachine(new Point(buttonA[0], buttonA[1]), new Point(buttonB[0], buttonB[1]), new Point(prize[0], prize[1])));
        }

        return clawMachines.Select(x => SolveMachine(x)).Where(x => x is not null).Select(x => (x.Value.A * 3) +  (x.Value.B * 1)).Sum();
    }

    private static (long A, long B)? SolveMachine(ClawMachine machine)
    {
        foreach (var aIndex in Enumerable.Range(1, 100))
        {
            foreach (var bIndex in Enumerable.Range(1, 100))
            {
                var aButton = new Point(machine.ButtonA.X * aIndex, machine.ButtonA.Y * aIndex);
                var bButton = new Point(machine.ButtonB.X * bIndex, machine.ButtonB.Y * bIndex);
                var total = new Point(aButton.X + bButton.X, aButton.Y + bButton.Y);

                if (total == machine.Prize)
                {
                    return (aIndex, bIndex);
                }
            }
        }

        return null;
    } 
}