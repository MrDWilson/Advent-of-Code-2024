namespace AdventOfCode.Models;

public enum SolutionType
{
    First,
    Second
}

public enum RunType
{
    Test,
    Full
}

public class SolutionOptions
{
    public const string Solution = "Solution";
    public int Day { get; set; } = 1;
    public SolutionType SolutionType { get; set; } = SolutionType.First;
    public RunType RunType { get; set; } = RunType.Test;
}