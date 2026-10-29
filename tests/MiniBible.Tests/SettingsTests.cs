using MiniBible.Core;

namespace MiniBible.Tests;

public static class SettingsTests
{
    public static void Test_DefaultsAndBounds()
    {
        var defaults = new AppSettings().Normalize();
        AssertEx.Equal(340, defaults.WindowWidth);
        AssertEx.Equal(240, defaults.WindowHeight);
        AssertEx.Equal(5, defaults.VersesPerPage);
        AssertEx.Equal(ThemeMode.System, defaults.Theme);
        var bounded = new AppSettings { WindowWidth=1, WindowHeight=9999, VersesPerPage=99, FontSize=2, FontFamily="맑은 고딕" }.Normalize();
        AssertEx.Equal(300, bounded.WindowWidth);
        AssertEx.Equal(1200, bounded.WindowHeight);
        AssertEx.Equal(10, bounded.VersesPerPage);
        AssertEx.Equal(9f, bounded.FontSize);
        AssertEx.Equal("맑은 고딕", bounded.FontFamily);
    }

    public static void Test_RoundTripAndCorruptFallback()
    {
        var dir = Path.Combine(Environment.CurrentDirectory, "work", "MiniBibleTests", Guid.NewGuid().ToString("N"));
        var file = Path.Combine(dir, "settings.json");
        try
        {
            var store = new SettingsStore();
            store.Save(file, new AppSettings { Theme=ThemeMode.Dark, FontFamily="굴림", VersesPerPage=1, LastBook="가상권", LastChapter=2 });
            var loaded = store.Load(file);
            AssertEx.Equal(ThemeMode.Dark, loaded.Theme);
            AssertEx.Equal("가상권", loaded.LastBook);
            AssertEx.Equal("굴림", loaded.FontFamily);
            AssertEx.Equal(1, loaded.VersesPerPage);
            File.WriteAllText(file, "{broken");
            AssertEx.Equal(340, store.Load(file).WindowWidth);
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }
}
