namespace AdventOfCode.Solutions;

public interface ISolution
{
    public int Day { get; }
    public Task<long> Solve();
}