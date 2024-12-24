using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day24(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 24;

    private Dictionary<string, bool> Values = null!;
    private List<((string, string) OpValues, Operation Op, string Destination)> Ops = null!;

    private enum Operation { And, Or, Xor };
    private static bool IntToBool(int value) => value == 1;
    private static int BoolToInt(bool value) => value ? 1 : 0;
    public async Task<string> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);

        var initialValueLines = lines.TakeWhile(x => x.Contains(':'));
        var operationLines = lines.SkipWhile(x => x.Contains(':')).Where(x => !string.IsNullOrWhiteSpace(x));

        Values = initialValueLines.Select(x => x.Split(": ")).ToDictionary(x => x[0], x => IntToBool(int.Parse(x[1])));
        Ops = operationLines.Select(x => x.Split(" -> ")).Select(x =>
        {
            var operation = x[0].Split(' ');
            var opValues = (operation[0], operation[2]);
            var op = operation[1] switch
            {
                "AND" => Operation.And,
                "OR" => Operation.Or,
                "XOR" => Operation.Xor,
                _ => throw new Exception("Invalid operation")
            };

            return (OpValues: opValues, Op: op, Destination: x[1]);
        }).ToList();

        if (options.Value.SolutionType is SolutionType.First)
        {
            foreach (var op in Ops)
            {
                SolveBit(op);
            }

            var bits = Values.Where(x => x.Key.StartsWith('z')).OrderByDescending(x => x.Key).Select(x => x.Value);
            return Convert.ToInt64(string.Join("", bits.Select(BoolToInt)), 2).ToString();
        }
        else
        {
            return FindIncorrect();
        }
    }

    private string FindIncorrect()
    {
        var wrong = Ops.Where(g => g.Destination.StartsWith('z') && g.Destination is not "z45" && g.Op is not Operation.Xor).Select(g => g.Destination).ToHashSet();

        wrong = wrong.Union(Ops.Where(g => !g.Destination.StartsWith('z')
                                             && g.OpValues.Item1.First() is not ('x' or 'y')
                                             && g.OpValues.Item2.First() is not ('x' or 'y')
                                             && g.Op is Operation.Xor).Select(g => g.Destination)).ToHashSet();

        foreach (var (OpValues, Op, Destination) in Ops.Where(g => g.Op is Operation.And && g.OpValues.Item1 is not "x00" && g.OpValues.Item2 is not "x00"))
        {
            foreach (var right in Ops)
            {
                if ((Destination == right.OpValues.Item1 || Destination == right.OpValues.Item2) && right.Op is not Operation.Or)
                {
                    wrong.Add(Destination);
                }
            }
        }

        foreach (var (OpValues, Op, Destination) in Ops.Where(g => g.Op is Operation.Xor))
        {
            foreach (var right in Ops)
            {
                if ((Destination == right.OpValues.Item1 || Destination == right.OpValues.Item2) && right.Op is Operation.Or)
                {
                    wrong.Add(Destination);
                }
            }
        }

        return string.Join(',', wrong.Order());
    }

    private void SolveBit(((string, string) OpValues, Operation Op, string Destination) bit)
    {
        if (Values.ContainsKey(bit.Destination)) return;

        if (!Values.TryGetValue(bit.OpValues.Item1, out bool op1))
        {
            var op1Bit = Ops.First(x => x.Destination == bit.OpValues.Item1);
            SolveBit(op1Bit);
            op1 = Values[bit.OpValues.Item1];
        }

        if (!Values.TryGetValue(bit.OpValues.Item2, out bool op2))
        {
            var op2Bit = Ops.First(x => x.Destination == bit.OpValues.Item2);
            SolveBit(op2Bit);
            op2 = Values[bit.OpValues.Item2];
        }

        Values[bit.Destination] = bit.Op switch
        {
            Operation.And => op1 & op2,
            Operation.Or => op1 | op2,
            Operation.Xor => op1 ^ op2,
            _ => throw new Exception("Invalid operation")
        };
    }
}