namespace AdventOfCode.Helpers;

public static class Debug
{
    private static string FlattenStructure<T>(IEnumerable<T> v, string s) => string.Join(s, v);

    private static void WriteLine(IEnumerable<string> values)
    {
        if (!values.Any())
        {
            Console.WriteLine("Empty");
            return;
        }

        Console.WriteLine(FlattenStructure(values, Environment.NewLine));
    }

    public static void WriteLine<T>(IEnumerable<T> values) where T : struct
    {
        if (!values.Any())
        {
            Console.WriteLine("Empty");
            return;
        }

        Console.WriteLine(FlattenStructure(values, ", "));
    }

    public static void WriteLine<T>(IEnumerable<IEnumerable<T>> values)
    {
        WriteLine(values.Select(x => FlattenStructure(x, ", ")));
    }
}