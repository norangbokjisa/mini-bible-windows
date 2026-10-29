namespace MiniBible.Core;

public enum ThemeMode { System, Light, Dark }

public sealed class AppSettings
{
    public int WindowWidth { get; set; } = 340;
    public int WindowHeight { get; set; } = 240;
    public int WindowX { get; set; } = -1;
    public int WindowY { get; set; } = -1;
    public int VersesPerPage { get; set; } = 5;
    public float FontSize { get; set; } = 12f;
    public string FontFamily { get; set; } = "맑은 고딕";
    public ThemeMode Theme { get; set; } = ThemeMode.System;
    public bool TopMost { get; set; }
    public string LastBook { get; set; } = "";
    public int LastChapter { get; set; } = 1;
    public int LastVerse { get; set; } = 1;

    public AppSettings Normalize()
    {
        WindowWidth = Math.Clamp(WindowWidth, 300, 1600);
        WindowHeight = Math.Clamp(WindowHeight, 220, 1200);
        VersesPerPage = Math.Clamp(VersesPerPage, 1, 10);
        FontSize = Math.Clamp(FontSize, 9f, 28f);
        FontFamily = string.IsNullOrWhiteSpace(FontFamily) ? "맑은 고딕" : FontFamily.Trim();
        LastChapter = Math.Max(1, LastChapter);
        LastVerse = Math.Max(1, LastVerse);
        if (!Enum.IsDefined(Theme)) Theme = ThemeMode.System;
        return this;
    }
}
