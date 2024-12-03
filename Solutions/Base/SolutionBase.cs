using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions.Base;

public abstract class SolutionBase(IFileLoader _loader, IOptions<SolutionOptions> _options) : ISolution
{
    private protected IFileLoader Loader => _loader;
    private protected SolutionOptions Options => _options.Value;
    public abstract int Day { get; }
    public abstract Task Solve();
}