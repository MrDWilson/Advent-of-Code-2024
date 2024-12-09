using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day7(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 7;

    public async Task<long> Solve()
    {
        var lines = await loader.Load<string>(Day, options.Value.SolutionType, options.Value.RunType);
        var items = lines.ToDictionary(x => long.Parse(x.First().TrimEnd(':')), x => x.Skip(1).Select(y => long.Parse(y)));

        return (options.Value.SolutionType switch
        {
            SolutionType.First => ProcessItems(items, [Operators.Add, Operators.Multiply]),
            SolutionType.Second => ProcessItems(items, [Operators.Add, Operators.Multiply, Operators.Combine]),
            _ => throw new ArgumentOutOfRangeException(nameof(options.Value.SolutionType))
        }).Sum();
    }

    private static IEnumerable<long> ProcessItems(Dictionary<long, IEnumerable<long>> items, Operators[] operators)
    {
        foreach (var item in items)
        {
            foreach (var ops in GetOperators(operators, item.Value.Count() - 1))
            {
                if (CalculateValue(item.Value, ops) is long value && value == item.Key)
                {
                    yield return value;
                    break;
                }
            }
        }
    }

    private static long CalculateValue(IEnumerable<long> values, IEnumerable<Operators> operators)
    {
        return values.Skip(1).Index().Aggregate(values.First(), (x, y) => 
        {
            return operators.ElementAt(y.Index) switch
            {
                Operators.Add => x + y.Item,
                Operators.Multiply => x * y.Item,
                Operators.Combine => long.Parse(x.ToString() + y.Item.ToString()),
                _ => throw new ArgumentOutOfRangeException(nameof(values))
            };
        });
    }

    private enum Operators { Add, Multiply, Combine };
    private static IEnumerable<IEnumerable<Operators>> GetOperators(Operators[] states, int length)
    {
        if (length <= 0)
        {
            yield break;
        }

        foreach (var combination in GenerateOperators(states, length))
        {
            yield return combination;
        }
    }

    private static IEnumerable<IEnumerable<Operators>> GenerateOperators(Operators[] states, int length)
    {
        if (length is 0)
        {
            yield return Enumerable.Empty<Operators>();
            yield break;
        }

        foreach (var seq in GenerateOperators(states, length - 1))
        {
            foreach (var state in states)
            {
                yield return seq.Concat([state]);
            }
        }
    }
}