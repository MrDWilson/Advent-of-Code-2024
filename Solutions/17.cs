using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day17(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 17;

    private class Program
    {
        public int RegisterA;
        public int RegisterB;
        public int RegisterC;
        public required List<int> Instructions;
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
            int newRegA = 0;
            while (true)
            {
                newRegA++;
                program.RegisterA = newRegA;
                var broken = false;
                var itemsMatched = 0;
                foreach (var result in RunProgram().Index())
                {
                    if (program.Instructions[result.Index] != result.Item)
                    {
                        broken = true;
                        break;
                    }
                    else itemsMatched++;
                }

                if (broken || itemsMatched != program.Instructions.Count) continue;

                return newRegA.ToString();
            }
        } 
    }

    private IEnumerable<int> RunProgram()
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

                pointer = value;
                continue;
            }

            // Our output instruction
            if (instruction is 5)
            {
                yield return Out(value);
                pointer += 2;
                continue;
            }

            Action<int> op = instruction switch
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

    private void Adv(int combo)
    {
        var comboValue = GetComboValue(combo);
        var divResult = program.RegisterA / Math.Pow(2, comboValue);
        program.RegisterA = (int)divResult;
    }

    private void Bxl(int literal)
    {
        program.RegisterB ^= literal;
    }

    private void Bst(int combo)
    {
        var comboValue = GetComboValue(combo);
        program.RegisterB = comboValue % 8;
    }

    private void Bxc(int _)
    {
        program.RegisterB ^= program.RegisterC;
    }

    private int Out(int combo)
    {
        var comboValue = GetComboValue(combo);
        return comboValue % 8;
    }

    private void Bdv(int combo)
    {
        var comboValue = GetComboValue(combo);
        var divResult = program.RegisterA / Math.Pow(2, comboValue);
        program.RegisterB = (int)divResult;
    }

    private void Cdv(int combo)
    {
        var comboValue = GetComboValue(combo);
        var divResult = program.RegisterA / Math.Pow(2, comboValue);
        program.RegisterC = (int)divResult;
    }

    private int GetComboValue(int combo)
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
        int registerA = 0, registerB = 0, registerC = 0;
        List<int> instructions = [];

        static int ParseLine(string x) => int.Parse(x.Split(":")[1].Trim()); 
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
                instructions = line.Split(":")[1].Split(",").Select(x => int.Parse(x.Trim())).ToList();
            }
        }

        return new Program() { RegisterA = registerA, RegisterB = registerB, RegisterC = registerC, Instructions = instructions };
    }
}