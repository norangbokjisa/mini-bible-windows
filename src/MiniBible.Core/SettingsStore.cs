using System.Text.Json;

namespace MiniBible.Core;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public AppSettings Load(string path)
    {
        try
        {
            if (!File.Exists(path)) return new AppSettings();
            return (JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new AppSettings()).Normalize();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(string path, AppSettings settings)
    {
        var directory = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(directory);
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(settings.Normalize(), Options));
        File.Move(temp, path, true);
    }
}
