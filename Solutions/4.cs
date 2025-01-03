using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

public partial class Day4(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 4;

    public async Task<string> Solve()
    {
        
        var grid = await loader.LoadGrid<char>(Day, options.Value.SolutionType, options.Value.RunType);

        int rowCount = grid.Count;
        int colCount = grid[0].Count;

        int totalMatches;
        if (options.Value.SolutionType is SolutionType.First)
        {
            const string word = "XMAS";
            int wordLength = word.Length;

            totalMatches = new[] { Direction.Vertical, Direction.Horizontal, Direction.Diagonal }
                .SelectMany(direction => GeneratePositions(direction, rowCount, colCount, wordLength))
                .Select(positions => ExtractWord(grid, positions))
                .Count(extractedWord => extractedWord == word);
        }
        else
        {
            const string word = "MAS";
            int wordLength = word.Length;

            totalMatches = new[] { Direction.Diagonal }
                .SelectMany(direction => GeneratePositions(direction, rowCount, colCount, wordLength))
                .Select(positions => (positions: positions.ToList(), word: ExtractWord(grid, positions)))
                .Where(extractedWord => extractedWord.word == word)
                .GroupBy(x => x.positions[1])
                .Count(x => x.Count() is 2);
        }

        return totalMatches.ToString();
    }

    private enum Direction { Vertical, Horizontal, Diagonal }
    private static IEnumerable<IEnumerable<(int row, int col)>> GeneratePositions(Direction direction, int rowCount, int colCount, int wordLength)
    {
        var deltas = direction switch
        {
            Direction.Vertical => [(dx: 1, dy: 0), (dx: -1, dy: 0)],
            Direction.Horizontal => [(dx: 0, dy: 1), (dx: 0, dy: -1)],
            Direction.Diagonal =>
            [
                (dx: 1, dy: 1),
                (dx: -1, dy: -1),
                (dx: 1, dy: -1),
                (dx: -1, dy: 1)
            ],
            _ => Array.Empty<(int dx, int dy)>()
        };

        foreach (var (dx, dy) in deltas)
        {
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    var positions = Enumerable.Range(0, wordLength)
                        .Select(i => (row: row + i * dx, col: col + i * dy));

                    if (positions.All(pos => pos.row >= 0 && pos.row < rowCount && pos.col >= 0 && pos.col < colCount))
                    {
                        yield return positions;
                    }
                }
            }
        }
    }

    private static string ExtractWord(List<List<char>> grid, IEnumerable<(int row, int col)> positions)
    {
        return new string(positions.Select(pos => grid[pos.row][pos.col]).ToArray());
    }
}