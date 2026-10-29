using Microsoft.VisualBasic.FileIO;

namespace MiniBible.Core;

public static class CsvBibleImporter
{
    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["book"] = "book", ["권"] = "book",
        ["chapter"] = "chapter", ["장"] = "chapter",
        ["verse"] = "verse", ["절"] = "verse",
        ["text"] = "text", ["본문"] = "text"
    };

    public static BibleData Parse(TextReader reader)
    {
        var content = reader.ReadToEnd();
        using var parser = new TextFieldParser(new StringReader(content));
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;
        parser.TrimWhiteSpace = false;

        try
        {
            if (parser.EndOfData) throw new BibleImportException("CSV가 비어 있습니다.");
            var rawHeaders = parser.ReadFields() ?? [];
            if (rawHeaders.Length > 0) rawHeaders[0] = rawHeaders[0].TrimStart('﻿');
            var headers = rawHeaders.Select(h => HeaderAliases.TryGetValue(h.Trim(), out var value) ? value : h.Trim()).ToArray();
            var required = new[] { "book", "chapter", "verse", "text" };
            if (required.Any(name => Array.IndexOf(headers, name) < 0))
                throw new BibleImportException("필수 열(book, chapter, verse, text 또는 권, 장, 절, 본문)이 없습니다.");

            var rows = new List<Verse>();
            var seen = new HashSet<(string, int, int)>();
            var line = 1;
            while (!parser.EndOfData)
            {
                line++;
                var fields = parser.ReadFields() ?? [];
                if (fields.All(string.IsNullOrWhiteSpace)) continue;
                string Field(string name)
                {
                    var index = Array.IndexOf(headers, name);
                    return index < fields.Length ? fields[index].Trim() : "";
                }

                var book = Field("book");
                var text = Field("text");
                if (string.IsNullOrWhiteSpace(book) || string.IsNullOrWhiteSpace(text)
                    || !int.TryParse(Field("chapter"), out var chapter) || chapter <= 0
                    || !int.TryParse(Field("verse"), out var verse) || verse <= 0)
                    throw new BibleImportException($"{line}행의 권·장·절·본문 값을 확인하세요.");
                if (!seen.Add((book, chapter, verse)))
                    throw new BibleImportException($"{line}행에 중복 구절이 있습니다: {book} {chapter}:{verse}");
                rows.Add(new Verse(book, chapter, verse, text));
            }
            if (rows.Count == 0) throw new BibleImportException("가져올 구절이 없습니다.");
            var bookOrder = rows.Select(v => v.Book).Distinct().Select((book, index) => (book, index))
                .ToDictionary(x => x.book, x => x.index);
            var ordered = rows.OrderBy(v => bookOrder[v.Book]).ThenBy(v => v.Chapter).ThenBy(v => v.Number).ToList();
            return new BibleData(ordered);
        }
        catch (MalformedLineException ex)
        {
            throw new BibleImportException("CSV 인용부호 형식이 올바르지 않습니다.", ex);
        }
    }
}
