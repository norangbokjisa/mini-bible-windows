namespace MiniBible.Core;

public sealed class BibleData
{
    public BibleData(IReadOnlyList<Verse> verses)
    {
        Verses = verses;
    }

    public IReadOnlyList<Verse> Verses { get; }
}
