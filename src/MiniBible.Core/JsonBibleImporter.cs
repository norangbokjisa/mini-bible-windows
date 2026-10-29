using System.Text.Json;
using System.Text.RegularExpressions;

namespace MiniBible.Core;

public static class JsonBibleImporter
{
    private static readonly IReadOnlyDictionary<string, string> BookNames = new Dictionary<string, string>
    {
        ["창"]="창세기",["출"]="출애굽기",["레"]="레위기",["민"]="민수기",["신"]="신명기",
        ["수"]="여호수아",["삿"]="사사기",["룻"]="룻기",["삼상"]="사무엘상",["삼하"]="사무엘하",
        ["왕상"]="열왕기상",["왕하"]="열왕기하",["대상"]="역대상",["대하"]="역대하",["스"]="에스라",
        ["느"]="느헤미야",["에"]="에스더",["욥"]="욥기",["시"]="시편",["잠"]="잠언",
        ["전"]="전도서",["아"]="아가",["사"]="이사야",["렘"]="예레미야",["애"]="예레미야애가",
        ["겔"]="에스겔",["단"]="다니엘",["호"]="호세아",["욜"]="요엘",["암"]="아모스",
        ["옵"]="오바댜",["욘"]="요나",["미"]="미가",["나"]="나훔",["합"]="하박국",
        ["습"]="스바냐",["학"]="학개",["슥"]="스가랴",["말"]="말라기",["마"]="마태복음",
        ["막"]="마가복음",["눅"]="누가복음",["요"]="요한복음",["행"]="사도행전",["롬"]="로마서",
        ["고전"]="고린도전서",["고후"]="고린도후서",["갈"]="갈라디아서",["엡"]="에베소서",["빌"]="빌립보서",
        ["골"]="골로새서",["살전"]="데살로니가전서",["살후"]="데살로니가후서",["딤전"]="디모데전서",["딤후"]="디모데후서",
        ["딛"]="디도서",["몬"]="빌레몬서",["히"]="히브리서",["약"]="야고보서",["벧전"]="베드로전서",
        ["벧후"]="베드로후서",["요일"]="요한일서",["요이"]="요한이서",["요삼"]="요한삼서",["유"]="유다서",["계"]="요한계시록"
    };

    public static BibleData Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind == JsonValueKind.Object) return ParseFlatObject(document.RootElement);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                throw new BibleImportException("JSON 최상위 값은 구절 배열 또는 구절 주소 객체여야 합니다.");
            var rows = new List<Verse>();
            var seen = new HashSet<(string, int, int)>();
            var bookOrder = new Dictionary<string, int>();
            var index = 0;
            foreach (var item in document.RootElement.EnumerateArray())
            {
                index++;
                if (item.ValueKind != JsonValueKind.Object) throw new BibleImportException($"{index}번째 항목은 객체여야 합니다.");
                var book = ReadString(item, "book", "권");
                var text = ReadString(item, "text", "본문");
                var chapter = ReadInt(item, "chapter", "장");
                var verse = ReadInt(item, "verse", "절");
                if (string.IsNullOrWhiteSpace(book) || string.IsNullOrWhiteSpace(text) || chapter <= 0 || verse <= 0)
                    throw new BibleImportException($"{index}번째 항목의 권·장·절·본문 값을 확인하세요.");
                if (!seen.Add((book, chapter, verse))) throw new BibleImportException($"{index}번째 항목에 중복 구절이 있습니다: {book} {chapter}:{verse}");
                if (!bookOrder.ContainsKey(book)) bookOrder[book] = bookOrder.Count;
                var endVerse = ReadOptionalInt(item, "verseEnd", "끝절");
                rows.Add(new(book, chapter, verse, text, endVerse > verse ? endVerse : null));
            }
            if (rows.Count == 0) throw new BibleImportException("가져올 구절이 없습니다.");
            return new BibleData(rows.OrderBy(v => bookOrder[v.Book]).ThenBy(v => v.Chapter).ThenBy(v => v.Number).ToList());
        }
        catch (BibleImportException) { throw; }
        catch (JsonException ex) { throw new BibleImportException("JSON 형식이 올바르지 않습니다.", ex); }
    }

    private static BibleData ParseFlatObject(JsonElement root)
    {
        var rows = new List<Verse>();
        var seen = new HashSet<(string, int, int)>();
        var pattern = new Regex(@"^([^0-9:]+)([0-9]+):([0-9]+)(?:-([0-9]+))?$", RegexOptions.CultureInvariant);
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name == "요18:이")
            {
                if (rows.Count == 0 || rows[^1].Book != "요한복음" || rows[^1].Chapter != 18 || rows[^1].Number != 38
                    || property.Value.ValueKind != JsonValueKind.String)
                    throw new BibleImportException("요18:이 이어쓰기 항목의 위치를 확인하세요.");
                var continuation = property.Value.GetString()?.Trim() ?? "";
                if (continuation.Length == 0) throw new BibleImportException("요18:이 이어쓰기 본문이 비어 있습니다.");
                rows[^1] = rows[^1] with { Text = rows[^1].Text.TrimEnd() + " " + continuation };
                continue;
            }
            var match = pattern.Match(property.Name);
            if (!match.Success || property.Value.ValueKind != JsonValueKind.String)
                throw new BibleImportException($"구절 주소 형식을 확인하세요: {property.Name}");
            var code = match.Groups[1].Value;
            if (!BookNames.TryGetValue(code, out var book)) throw new BibleImportException($"알 수 없는 성경 권 약어입니다: {code}");
            var chapter = int.Parse(match.Groups[2].Value);
            var verse = int.Parse(match.Groups[3].Value);
            int? endVerse = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : null;
            var text = property.Value.GetString()?.Trim() ?? "";
            if (text.Length == 0 || (endVerse.HasValue && endVerse < verse))
                throw new BibleImportException($"구절 값을 확인하세요: {property.Name}");
            if (!seen.Add((book, chapter, verse))) throw new BibleImportException($"중복 구절이 있습니다: {property.Name}");
            rows.Add(new(book, chapter, verse, text, endVerse));
        }
        if (rows.Count == 0) throw new BibleImportException("가져올 구절이 없습니다.");
        return new BibleData(rows);
    }

    private static string ReadString(JsonElement item, string english, string korean)
    {
        if (TryGet(item, english, korean, out var value) && value.ValueKind == JsonValueKind.String) return value.GetString()?.Trim() ?? "";
        return "";
    }

    private static int ReadInt(JsonElement item, string english, string korean)
    {
        if (!TryGet(item, english, korean, out var value)) return 0;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)) return number;
        if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out number)) return number;
        return 0;
    }

    private static int ReadOptionalInt(JsonElement item, string english, string korean)
        => TryGet(item, english, korean, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) ? number : 0;

    private static bool TryGet(JsonElement item, string english, string korean, out JsonElement value)
    {
        if (item.TryGetProperty(english, out value) || item.TryGetProperty(korean, out value)) return true;
        foreach (var property in item.EnumerateObject())
            if (property.Name.Equals(english, StringComparison.OrdinalIgnoreCase)) { value = property.Value; return true; }
        value = default; return false;
    }
}
