using AdventOfCode.Models;
using AdventOfCode.Services;

namespace AdventOfCode.Solutions.Base;

public abstract class SolutionBase(IFileLoader _loader) : ISolution
{
    private protected IFileLoader Loader => _loader;
    public abstract int Day { get; }
    public abstract Task Solve(SolutionType solutionType, RunType runType);
}