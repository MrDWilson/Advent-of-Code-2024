using System.Drawing;
using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day13(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 13;

    public record LongPoint(long X, long Y);
    public record ClawMachine(LongPoint ButtonA, LongPoint ButtonB, LongPoint Prize);
    public async Task<long> Solve()
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

        return clawMachines.Select(x => options.Value.SolutionType is SolutionType.First ? SolveMachine(x) : FindClosestCounts(x))
            .Where(x => x is not null).Select(x => (x.Value.A * 3) +  (x.Value.B * 1)).Sum();
    }

    /// <summary>
    /// Finds the closest counts of Button A and B presses to reach the target.
    /// Minimizes the total number of presses (n + m) and favours Button A.
    /// </summary>
    public static (long A, long B)? FindClosestCounts(ClawMachine clawMachine)
    {
        // Calculate approximate m from the system
        double mApprox = (94.0 * clawMachine.Prize.Y - 34.0 * clawMachine.Prize.X) / 5550.0;

        // Round mApprox to the nearest integer
        long m = (long)Math.Round(mApprox);

        // Iterate around m to find the best approximation
        // Limit the search range to m +/- 1000 for practicality
        long searchRange = 1000;
        long bestN = 0, bestM = 0;
        long smallestResidual = long.MaxValue;

        for (long delta = -searchRange; delta <= searchRange; delta++)
        {
            long currentM = m + delta;
            if (currentM < 0)
                continue;

            // Calculate n based on current m
            long numeratorN = clawMachine.Prize.X - clawMachine.ButtonB.X * currentM;
            if (numeratorN < 0)
                continue;
            if (numeratorN % clawMachine.ButtonA.X != 0)
                continue;
            long currentN = numeratorN / clawMachine.ButtonA.X;

            // Calculate achieved Y
            long achievedY = currentN * clawMachine.ButtonA.Y + currentM * clawMachine.ButtonB.Y;

            // Calculate residual for Y
            long residualY = Math.Abs(clawMachine.Prize.Y - achievedY);

            // Calculate residual for X
            long achievedX = currentN * clawMachine.ButtonA.X + currentM * clawMachine.ButtonB.X;
            long residualX = Math.Abs(clawMachine.Prize.X - achievedX);

            // Total residual
            long totalResidual = residualX + residualY;

            // Check if this is the best so far
            if (totalResidual < smallestResidual)
            {
                smallestResidual = totalResidual;
                bestN = currentN;
                bestM = currentM;
            }

            // Early exit if perfect match is found
            if (totalResidual == 0)
                break;
        }

        // Check if a valid approximation was found
        if (smallestResidual < long.MaxValue)
            return (bestN, bestM);

        return null;
    }

    private static (long A, long B)? SolveMachine(ClawMachine machine)
    {
        foreach (var aIndex in Enumerable.Range(1, 100))
        {
            foreach (var bIndex in Enumerable.Range(1, 100))
            {
                var aButton = new LongPoint(machine.ButtonA.X * aIndex, machine.ButtonA.Y * aIndex);
                var bButton = new LongPoint(machine.ButtonB.X * bIndex, machine.ButtonB.Y * bIndex);
                var total = new LongPoint(aButton.X + bButton.X, aButton.Y + bButton.Y);

                if (total == machine.Prize)
                {
                    return (aIndex, bIndex);
                }
            }
        }

        return null;
    } 
}