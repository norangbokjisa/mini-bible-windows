using MiniBible.Core;
using System.Text.Json;

namespace MiniBible.App;

public static class ImportService
{
    public static BibleData ValidateAndInstall(string source, string destination)
    {
        BibleData data;
        if (Path.GetExtension(source).Equals(".json", StringComparison.OrdinalIgnoreCase))
            data = JsonBibleImporter.Parse(File.ReadAllText(source));
        else
        {
            using var reader = new StreamReader(source, detectEncodingFromByteOrderMarks: true);
            data = CsvBibleImporter.Parse(reader);
        }
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        var temp = destination + ".tmp";
        var canonical = data.Verses.Select(v => new { book=v.Book, chapter=v.Chapter, verse=v.Number, verseEnd=v.EndNumber, text=v.Text });
        File.WriteAllText(temp, JsonSerializer.Serialize(canonical, new JsonSerializerOptions { WriteIndented=true }));
        File.Move(temp, destination, true);
        return data;
    }
}
