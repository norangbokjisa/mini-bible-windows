using MiniBible.Core;

namespace MiniBible.Tests;

public static class JsonBibleImporterTests
{
    public static void Test_EnglishAndKoreanProperties()
    {
        var json = """
        [
          {"book":"가상A","chapter":1,"verse":2,"text":"둘"},
          {"권":"가상A","장":1,"절":1,"본문":"하나"}
        ]
        """;
        var data = JsonBibleImporter.Parse(json);
        AssertEx.Equal("하나,둘", string.Join(",", data.Verses.Select(v => v.Text)));
    }

    public static void Test_InvalidJsonAndDuplicateAreRejected()
    {
        AssertEx.Throws<BibleImportException>(() => JsonBibleImporter.Parse("{broken"));
        var duplicate = """[{"book":"가상","chapter":1,"verse":1,"text":"하나"},{"book":"가상","chapter":1,"verse":1,"text":"둘"}]""";
        AssertEx.Throws<BibleImportException>(() => JsonBibleImporter.Parse(duplicate));
    }

    public static void Test_FlatReferenceObjectSupportsBookCodesRangesAndJohn18Continuation()
    {
        var json = """{"창1:1":"첫째","신6:18-19":"합쳐진 본문","요18:38":"앞부분","요18:이":"뒷부분"}""";
        var data = JsonBibleImporter.Parse(json);
        AssertEx.Equal(3, data.Verses.Count);
        AssertEx.Equal("창세기", data.Verses[0].Book);
        AssertEx.Equal(19, data.Verses[1].EndNumber);
        AssertEx.Equal(38, data.Verses[2].Number);
        AssertEx.Equal("앞부분 뒷부분", data.Verses[2].Text);
    }
}
