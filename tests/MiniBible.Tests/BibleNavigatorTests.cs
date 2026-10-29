using MiniBible.Core;

namespace MiniBible.Tests;

public static class BibleNavigatorTests
{
    private static BibleNavigator Navigator() => new(new BibleData([
        new("가상A",1,1,"A11"), new("가상A",1,2,"A12"), new("가상A",1,3,"A13"),
        new("가상A",2,1,"A21"), new("가상A",2,2,"A22"), new("가상B",1,1,"B11")
    ]));

    public static void Test_WindowAndShortFinalWindow()
    {
        var nav = Navigator();
        AssertEx.Equal("A13", nav.GetWindow(new("가상A", 1, 1), 3)[2].Text);
        AssertEx.Equal(2, nav.GetWindow(new("가상A", 2, 2), 5).Count);
    }

    public static void Test_MoveCrossesChapterAndBookBoundaries()
    {
        var nav = Navigator();
        AssertEx.Equal(new VerseReference("가상A", 2, 1), nav.MoveWindow(new("가상A", 1, 1), 3, 1));
        AssertEx.Equal(new VerseReference("가상B", 1, 1), nav.MoveWindow(new("가상A", 2, 1), 2, 1));
        AssertEx.Equal(new VerseReference("가상A", 1, 2), nav.MoveWindow(new("가상A", 2, 2), 3, -1));
    }

    public static void Test_ListsAndInvalidReference()
    {
        var nav = Navigator();
        AssertEx.Equal("가상A,가상B", string.Join(",", nav.GetBooks()));
        AssertEx.Equal("1,2", string.Join(",", nav.GetChapters("가상A")));
        AssertEx.Equal(new VerseReference("가상A", 1, 1), nav.Clamp(new("없는권", 99, 99)));
    }
}
