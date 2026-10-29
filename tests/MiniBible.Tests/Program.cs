using System.Reflection;
using MiniBible.Core;

if (args.Length == 2 && args[0] == "--probe-json")
{
    var data = JsonBibleImporter.Parse(File.ReadAllText(args[1]));
    Console.WriteLine($"JSON_PROBE_OK verses={data.Verses.Count} books={data.Verses.Select(v => v.Book).Distinct().Count()}");
    return 0;
}

var failures = new List<string>();
var tests = Assembly.GetExecutingAssembly().GetTypes()
    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
    .Where(m => m.Name.StartsWith("Test_", StringComparison.Ordinal))
    .OrderBy(m => m.DeclaringType!.Name).ThenBy(m => m.Name);

foreach (var test in tests)
{
    try
    {
        test.Invoke(null, null);
        Console.WriteLine($"PASS {test.DeclaringType!.Name}.{test.Name}");
    }
    catch (TargetInvocationException ex)
    {
        failures.Add($"{test.DeclaringType!.Name}.{test.Name}: {ex.InnerException?.Message}");
        Console.WriteLine($"FAIL {failures[^1]}");
    }
}

Console.WriteLine($"{tests.Count() - failures.Count}/{tests.Count()} tests passed");
return failures.Count == 0 ? 0 : 1;
