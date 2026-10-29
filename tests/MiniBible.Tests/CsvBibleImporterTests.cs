using MiniBible.Core;

namespace MiniBible.Tests;

public static class CsvBibleImporterTests
{
    public static void Test_EnglishHeadersAndQuotedFields()
    {
        var csv = "book,chapter,verse,text\n가상권,1,1,\"쉼표, 따옴표 \"\"안\"\"\"\n가상권,1,2,\"두 줄\n본문\"";
        var data = CsvBibleImporter.Parse(new StringReader(csv));
        AssertEx.Equal(2, data.Verses.Count);
        AssertEx.Equal("쉼표, 따옴표 \"안\"", data.Verses[0].Text);
        AssertEx.Equal("두 줄\n본문", data.Verses[1].Text);
    }

    public static void Test_KoreanHeadersAndBom()
    {
        var data = CsvBibleImporter.Parse(new StringReader("\uFEFF권,장,절,본문\n가상권,2,3,내용"));
        AssertEx.Equal(new Verse("가상권", 2, 3, "내용"), data.Verses.Single());
    }

    public static void Test_InvalidRowsAreRejected()
    {
        AssertEx.Throws<BibleImportException>(() => CsvBibleImporter.Parse(new StringReader("book,chapter,verse,text\n가상권,0,1,내용")));
        AssertEx.Throws<BibleImportException>(() => CsvBibleImporter.Parse(new StringReader("book,chapter,verse,text\n가상권,1,1,")));
        AssertEx.Throws<BibleImportException>(() => CsvBibleImporter.Parse(new StringReader("book,chapter,text\n가상권,1,내용")));
    }

    public static void Test_DuplicateReferenceIsRejected()
    {
        var csv = "book,chapter,verse,text\n가상권,1,1,하나\n가상권,1,1,둘";
        AssertEx.Throws<BibleImportException>(() => CsvBibleImporter.Parse(new StringReader(csv)));
    }

    public static void Test_RowsAreSortedWithinFirstSeenBookOrder()
    {
        var csv = "book,chapter,verse,text\n가상B,2,1,B21\n가상A,1,1,A11\n가상B,1,2,B12\n가상B,1,1,B11";
        var data = CsvBibleImporter.Parse(new StringReader(csv));
        AssertEx.Equal("B11,B12,B21,A11", string.Join(",", data.Verses.Select(v => v.Text)));
    }
}
