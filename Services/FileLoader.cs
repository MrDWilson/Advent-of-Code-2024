using AdventOfCode.Models;

namespace AdventOfCode.Services;

public interface IFileLoader
{
    Task<string[]> Load(int day, SolutionType solutionType, RunType runType);
}

public class FileLoader : IFileLoader
{
    public async Task<string[]> Load(int day, SolutionType solutionType, RunType runType)
    {
        var filetype = (solutionType, runType) switch
        {
            (SolutionType.First, RunType.Test) => day + "-1-test",
            (SolutionType.Second, RunType.Test) => day + "-2-test",
            _ => day.ToString()
        };

        var filename = $"{filetype}.txt";
        return await File.ReadAllLinesAsync(Path.Combine("Files", filename));
    }
}