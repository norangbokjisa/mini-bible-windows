namespace MiniBible.Core;

public sealed class BibleNavigator
{
    private readonly IReadOnlyList<Verse> verses;

    public BibleNavigator(BibleData data) => verses = data.Verses;

    public IReadOnlyList<string> GetBooks() => verses.Select(v => v.Book).Distinct().ToList();
    public IReadOnlyList<int> GetChapters(string book) => verses.Where(v => v.Book == book).Select(v => v.Chapter).Distinct().Order().ToList();
    public IReadOnlyList<int> GetVerses(string book, int chapter) => verses.Where(v => v.Book == book && v.Chapter == chapter).Select(v => v.Number).Order().ToList();

    public VerseReference Clamp(VerseReference requested)
    {
        var exact = verses.FirstOrDefault(v => v.Book == requested.Book && v.Chapter == requested.Chapter && v.Number == requested.Verse);
        var value = exact ?? verses.First();
        return new(value.Book, value.Chapter, value.Number);
    }

    public IReadOnlyList<Verse> GetWindow(VerseReference start, int count)
    {
        var index = IndexOf(Clamp(start));
        return verses.Skip(index).Take(Math.Clamp(count, 1, 10)).ToList();
    }

    public VerseReference MoveWindow(VerseReference start, int count, int direction)
    {
        var index = IndexOf(Clamp(start));
        index = Math.Clamp(index + Math.Clamp(count, 1, 10) * Math.Sign(direction), 0, verses.Count - 1);
        var value = verses[index];
        return new(value.Book, value.Chapter, value.Number);
    }

    private int IndexOf(VerseReference reference)
        => verses.Select((v, i) => (v, i)).First(x => x.v.Book == reference.Book && x.v.Chapter == reference.Chapter && x.v.Number == reference.Verse).i;
}
