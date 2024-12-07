using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day5(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 5;

    public async Task<long> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);

        var instructionLines = lines.TakeWhile(x => x.Contains('|')).ToList();
        var printInstructions = lines.Skip(instructionLines.Count).Where(line => !string.IsNullOrWhiteSpace(line));

        var rules = instructionLines
            .Select(line => line.Split('|', 2))
            .Select(parts => (Key: int.Parse(parts[0]), Value: int.Parse(parts[1])))
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Select(pair => pair.Value).ToList());

        var instructions = printInstructions
            .Select(line => line.Split(',').Select(int.Parse).ToList())
            .ToList();

        var orderedPages = instructions.Where(instruction => IsPageOrdered(instruction, rules));

        List<List<int>> items;
        if (options.Value.SolutionType is SolutionType.First)
        {
            items = orderedPages.ToList();
        }
        else
        {
            var orderedSet = new HashSet<List<int>>(orderedPages);
            var unorderedPages = instructions.Except(orderedSet);
            items = unorderedPages.Select(page => 
            {
                var clone = page.ToList();
                clone.Sort((x, y) =>
                {
                    if (rules.TryGetValue(x, out var xRules) && xRules.Contains(y))
                        return -1;
                    if (rules.TryGetValue(y, out var yRules) && yRules.Contains(x))
                        return 1;
                    return 0;
                });
                return clone;
            }).ToList();
        }

        return items.Sum(page => page[page.Count / 2]);
    }

    private static bool IsPageOrdered(List<int> instruction, Dictionary<int, List<int>> rules)
    {
        foreach ((var page, var index) in instruction.Select((x, i) => (x, i)))
        {
            if (!rules.TryGetValue(page, out var pageRules)) continue;
            else if (instruction.Any(x => x != page && pageRules.Any(y => y == x) && index > instruction.IndexOf(x)))
            {
                return false;
            }
        }

        return true;
    }
}