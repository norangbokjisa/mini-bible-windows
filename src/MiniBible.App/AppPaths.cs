namespace MiniBible.App;

public static class AppPaths
{
    public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiniBibleV2");
    public static string BibleData => Path.Combine(DataDirectory, "bible.json");
    public static string SettingsJson => Path.Combine(DataDirectory, "settings.json");
}
