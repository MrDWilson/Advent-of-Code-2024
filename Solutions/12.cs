using System.Drawing;
using AdventOfCode.Helpers;
using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day12(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 12;

    public async Task<long> Solve()
    {
        var grid = await loader.LoadTypedGrid<char>(Day, options.Value.SolutionType, options.Value.RunType);
        var plants = grid.GetUniqueItems();

        foreach (var plant in plants)
        {

        }

        return 1;
    }

    private IEnumerable<Point> GetAreas(Grid<char> grid, char c)
    {
        var points = grid.FindItems(c);
        return [];
    }
}