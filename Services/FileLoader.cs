using AdventOfCode.Helpers;
using AdventOfCode.Models;

namespace AdventOfCode.Services;

public interface IFileLoader
{
    Task<string> LoadRaw(int day, SolutionType solutionType, RunType runType);
    Task<List<string>> LoadLines(int day, SolutionType solutionType, RunType runType);
    Task<List<List<T>>> LoadGrid<T>(int day, SolutionType solutionType, RunType runType);
    Task<Grid<T>> LoadTypedGrid<T>(int day, SolutionType solutionType, RunType runType) where T : struct;
    Task<List<List<T>>> Load<T>(int day, SolutionType solutionType, RunType runType);
}

public class FileLoader : IFileLoader
{
    public async Task<string> LoadRaw(int day, SolutionType solutionType, RunType runType)
    {
        var filetype = (solutionType, runType) switch
        {
            (SolutionType.First, RunType.Test) => day + "-1",
            (SolutionType.Second, RunType.Test) => day + "-2",
            _ => day.ToString()
        };

        var filename = $"{filetype}.txt";
        return await File.ReadAllTextAsync(Path.Combine("Files", filename));
    }

    public async Task<List<string>> LoadLines(int day, SolutionType solutionType, RunType runType)
    {
        var raw = await LoadRaw(day, solutionType, runType);
        return [.. raw.Split("\n", StringSplitOptions.RemoveEmptyEntries)];
    }

    public async Task<List<List<T>>> LoadGrid<T>(int day, SolutionType solutionType, RunType runType)
    {
        var lines = await LoadLines(day, solutionType, runType);
        return lines.Select(x => x.Select(y => y.ToString()).Where(y => !string.IsNullOrWhiteSpace(y)).Select(y => ChangeType<T>(y)).ToList()).ToList();
    }

    public async Task<Grid<T>> LoadTypedGrid<T>(int day, SolutionType solutionType, RunType runType) where T : struct
    {
        return new Grid<T>(await LoadGrid<T>(day, solutionType, runType));
    }

    public async Task<List<List<T>>> Load<T>(int day, SolutionType solutionType, RunType runType)
    {
        var lines = await LoadLines(day, solutionType, runType);
        var lineItems = lines.Select(line => line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries));
        return lineItems.Select(x => x.Select(y => ChangeType<T>(y)).ToList()).ToList();
    }

    private static T ChangeType<T>(object obj)
    {
        return (T)Convert.ChangeType(obj, typeof(T));
    }
}