namespace MiniBible.Core;

public sealed record Verse(string Book, int Chapter, int Number, string Text, int? EndNumber = null)
{
    public string NumberLabel => EndNumber is > 0 ? $"{Number}-{EndNumber}" : Number.ToString();
}
public sealed record VerseReference(string Book, int Chapter, int Verse);

public sealed class BibleImportException : Exception
{
    public BibleImportException(string message) : base(message) { }
    public BibleImportException(string message, Exception inner) : base(message, inner) { }
}
