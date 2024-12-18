using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day13(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 13;

    public record LongPoint(long X, long Y);
    public record ClawMachine(LongPoint ButtonA, LongPoint ButtonB, LongPoint Prize);
    public async Task<string> Solve()
    {
        var input = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var blocks = input.Where(x => x.Trim() is { Length: > 0 }).Chunk(3);

        List<long> ProcessInput(string s, string splitChar)
        {
            return s.Split(":", StringSplitOptions.RemoveEmptyEntries)[1]
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => long.Parse(x.Split(splitChar)[1]))
                .ToList();
        }

        List<ClawMachine> clawMachines = [];
        foreach (var block in blocks)
        {
            var buttonA = ProcessInput(block[0], "+");
            var buttonB = ProcessInput(block[1], "+");
            var prize = ProcessInput(block[2], "=");

            var aButtonPoint = new LongPoint(buttonA[0], buttonA[1]);
            var bButtonPoint = new LongPoint(buttonB[0], buttonB[1]);
            var prizePoint = new LongPoint(prize[0], prize[1]);

            if (options.Value.SolutionType is SolutionType.Second)
            {
                prizePoint = new LongPoint(prizePoint.X + 10000000000000, prizePoint.Y + 10000000000000);
            }

            clawMachines.Add(new ClawMachine(aButtonPoint, bButtonPoint, prizePoint));
        }

        return clawMachines.Select(SolveEquation).Where(x => x is not 0).Sum().ToString();
    }

    private static long SolveEquation(ClawMachine clawMachine)
    {
        // ax, ay = (x,y) of button one
        // bx, by = (x,y) of button two
        // ac, bc = count of pressed for buttons
        // xx, xy = (x, y) of prize
        // ax * ac + bx * bc = xx // Prize X equation
        // ay * ac + by * bc = xy // Prize Y equation

        // Math time, start with Cramer's Rule
        // xy * ax - xx * ay
        var left = clawMachine.Prize.Y * clawMachine.ButtonA.X - clawMachine.Prize.X * clawMachine.ButtonA.Y;
        // by * ax - bx * ay
        var right = clawMachine.ButtonB.Y * clawMachine.ButtonA.X - clawMachine.ButtonB.X * clawMachine.ButtonA.Y;
        var B = left / right;

        // Now we have B, A becomes a bit easier
        var A = (clawMachine.Prize.X - B * clawMachine.ButtonB.X) / clawMachine.ButtonA.X;

        // Does this equation actually work?
        // Check our first button
        if (clawMachine.ButtonA.X * A + clawMachine.ButtonB.X * B != clawMachine.Prize.X) return 0;

        // Check our second button
        if (clawMachine.ButtonA.Y * A + clawMachine.ButtonB.Y * B != clawMachine.Prize.Y) return 0;

        return A * 3 + B;
    }
}