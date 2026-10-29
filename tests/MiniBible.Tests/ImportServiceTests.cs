using MiniBible.App;

namespace MiniBible.Tests;

public static class ImportServiceTests
{
    public static void Test_InvalidImportPreservesInstalledData()
    {
        var dir = Path.Combine(Environment.CurrentDirectory, "work", "import-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var source = Path.Combine(dir, "bad.csv");
        var destination = Path.Combine(dir, "installed.csv");
        try
        {
            File.WriteAllText(source, "bad,data");
            File.WriteAllText(destination, "keep me");
            AssertEx.Throws<Exception>(() => ImportService.ValidateAndInstall(source, destination));
            AssertEx.Equal("keep me", File.ReadAllText(destination));
        }
        finally { Directory.Delete(dir, true); }
    }

    public static void Test_ValidImportInstallsCopy()
    {
        var dir = Path.Combine(Environment.CurrentDirectory, "work", "import-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var source = Path.Combine(dir, "good.csv");
        var destination = Path.Combine(dir, "data", "installed.csv");
        try
        {
            File.WriteAllText(source, "book,chapter,verse,text\n가상권,1,1,가상 본문");
            var data = ImportService.ValidateAndInstall(source, destination);
            AssertEx.Equal(1, data.Verses.Count);
            AssertEx.True(File.Exists(destination));
        }
        finally { Directory.Delete(dir, true); }
    }

    public static void Test_JsonImportInstallsCanonicalDataFile()
    {
        var dir = Path.Combine(Environment.CurrentDirectory, "work", "import-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var source = Path.Combine(dir, "good.json");
        var destination = Path.Combine(dir, "data", "installed.json");
        try
        {
            File.WriteAllText(source, """[{"book":"가상권","chapter":1,"verse":1,"text":"가상 본문"}]""");
            var data = ImportService.ValidateAndInstall(source, destination);
            AssertEx.Equal("가상 본문", data.Verses.Single().Text);
            AssertEx.True(File.Exists(destination));
        }
        finally { Directory.Delete(dir, true); }
    }
}
