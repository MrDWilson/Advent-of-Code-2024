using AdventOfCode.Models;
using AdventOfCode.Services;
using Microsoft.Extensions.Options;

namespace AdventOfCode.Solutions;

class HashSetComparer<T> : IEqualityComparer<HashSet<T>>
{
    public bool Equals(HashSet<T>? x, HashSet<T>? y)
    {
        return (x is null && y is null) || x!.SequenceEqual(y!);
    }

    public int GetHashCode(HashSet<T> obj)
    {
        int hashcode = 0;
        foreach (T t in obj)
        {
            hashcode ^= t!.GetHashCode();
        }
        return hashcode;
    }
}

public partial class Day23(IFileLoader loader, IOptions<SolutionOptions> options) : ISolution
{
    public int Day => 23;

    public async Task<string> Solve()
    {
        var lines = await loader.LoadLines(Day, options.Value.SolutionType, options.Value.RunType);
        var pairs = lines.Select(x => x.Split("-").Take(2)).Select(x => (x.First(), x.Last()));

        Dictionary<string, List<string>> connections = [];
        foreach (var unique in pairs.Select(x => x.Item1).Union(pairs.Select(x => x.Item2)).Distinct())
        {
            List<string> conns = [];
            foreach (var pair in pairs)
            {
                if (pair.Item1 == unique) conns.Add(pair.Item2);
                if (pair.Item2 == unique) conns.Add(pair.Item1);
            }
            connections[unique] = conns;
        }

        if (options.Value.SolutionType is SolutionType.First)
        {
            HashSet<(string one, string two, string three)> groups = [];
            foreach(var potential in connections.Where(x => x.Value.Count > 1))
            {
                HashSet<HashSet<string>> matches = new(new HashSetComparer<string>());
                foreach (var link in potential.Value)
                {
                    var matchingLinks = potential.Value.Where(x => connections[x].Contains(link));
                    foreach (var matchingLink in matchingLinks)
                    {
                        matches.Add([potential.Key, link, matchingLink]);
                    }
                }

                if (matches.Count > 0)
                {
                    var combinations = matches.SelectMany(x => Combinations([.. x], 3));
                    foreach (var combo in combinations.Select(x => x.OrderBy(y => y)))
                    {
                        groups.Add((combo.ElementAt(0), combo.ElementAt(1), combo.ElementAt(2)));
                    }
                }
            }

            return groups.Count(x => x.one.StartsWith('t') || x.two.StartsWith('t') || x.three.StartsWith('t')).ToString();
        }
        else
        {
            HashSet<HashSet<string>> groups = new(new HashSetComparer<string>());
            foreach (var potential in connections)
            {
                if (groups.Any(x => x.Contains(potential.Key))) continue;

                HashSet<string> group = [potential.Key];
                foreach (var other in connections)
                {
                    if (group.All(x => connections[x].Contains(other.Key)))
                    {
                        group.Add(other.Key);
                    }
                }
                groups.Add(group);
            }

            return string.Join(",", groups.OrderByDescending(x => x.Count).First().OrderBy(x => x));
        }
    }

    public static IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> source, int k)
    {
        if (k is 0) yield return Array.Empty<T>();
        else
        {
            var i = 0;
            foreach (var item in source)
            {
                i++;
                foreach (var combination in Combinations(source.Skip(i), k - 1))
                    yield return new[] { item }.Concat(combination);
            }
        }
    }
}