using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day17(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 17;

    private class Program
    {
        public long RegisterA;
        public long RegisterB;
        public long RegisterC;
        public required List<long> Instructions;
    }

    private Program program = null!;
    public async Task<string> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        program = ParseInput(lines);

        if (options.Value.SolutionType is SolutionType.First)
        {
            return string.Join(",", RunProgram().ToList());
        }
        else
        {
            return SolveA(0, 0).Min().ToString();
        } 
    }

    private List<long> SolveA(long current, int depth)
    {
        List<long> res = [];
        if (depth > program.Instructions.Count) return res;
        var tmp = current << 3;
        foreach (var i in Enumerable.Range(0, 8))
        {
            program.RegisterA = tmp + i;
            program.RegisterB = program.RegisterC = 0;
            var tmpRes = RunProgram();
            if (tmpRes.SequenceEqual(program.Instructions.TakeLast(depth + 1)))
            {
                if (depth + 1 == program.Instructions.Count) res.Add(tmp + i);
                res.AddRange(SolveA(tmp + i, depth + 1));
            }
        }

        return res;
    } 

    private IEnumerable<long> RunProgram()
    {
        int pointer = 0;
        while (pointer < program.Instructions.Count)
        {
            var instruction = program.Instructions[pointer];
            var value = program.Instructions[pointer + 1];

            // The jump instruction
            if (instruction is 3)
            {
                if (program.RegisterA is 0)
                {
                    pointer += 2;
                    continue;
                }

                pointer = (int)value;
                continue;
            }

            // Our output instruction
            if (instruction is 5)
            {
                yield return Out(value);
                pointer += 2;
                continue;
            }

            Action<long> op = instruction switch
            {
                0 => Adv,
                1 => Bxl,
                2 => Bst,
                4 => Bxc,
                6 => Bdv,
                7 => Cdv,
                _ => throw new ArgumentOutOfRangeException(nameof(instruction))
            };

            op.Invoke(value);
            pointer += 2;
        }
    }

    private void Adv(long combo)
    {
        var comboValue = GetComboValue(combo);
        var divResult = program.RegisterA / Math.Pow(2, comboValue);
        program.RegisterA = (long)divResult;
    }

    private void Bxl(long literal)
    {
        program.RegisterB ^= literal;
    }

    private void Bst(long combo)
    {
        var comboValue = GetComboValue(combo);
        program.RegisterB = comboValue % 8;
    }

    private void Bxc(long _)
    {
        program.RegisterB ^= program.RegisterC;
    }

    private long Out(long combo)
    {
        var comboValue = GetComboValue(combo);
        return comboValue % 8;
    }

    private void Bdv(long combo)
    {
        var comboValue = GetComboValue(combo);
        var divResult = program.RegisterA / Math.Pow(2, comboValue);
        program.RegisterB = (long)divResult;
    }

    private void Cdv(long combo)
    {
        var comboValue = GetComboValue(combo);
        var divResult = program.RegisterA / Math.Pow(2, comboValue);
        program.RegisterC = (long)divResult;
    }

    private long GetComboValue(long combo)
    {
        return combo switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => 3,
            4 => program.RegisterA,
            5 => program.RegisterB,
            6 => program.RegisterC,
            _ => throw new ArgumentOutOfRangeException(nameof(combo))
        };
    }

    private static Program ParseInput(List<string> lines)
    {
        long registerA = 0, registerB = 0, registerC = 0;
        List<long> instructions = [];

        static long ParseLine(string x) => long.Parse(x.Split(":")[1].Trim()); 
        foreach (var line in lines)
        {
            if (line.StartsWith("Register A"))
            {
                registerA = ParseLine(line);
            }
            else if (line.StartsWith("Register B"))
            {
                registerB = ParseLine(line);
            }
            else if (line.StartsWith("Register C"))
            {
                registerC = ParseLine(line);
            }
            else if (line.StartsWith("Program"))
            {
                instructions = line.Split(":")[1].Split(",").Select(x => long.Parse(x.Trim())).ToList();
            }
        }

        return new Program() { RegisterA = registerA, RegisterB = registerB, RegisterC = registerC, Instructions = instructions };
    }
}