namespace MiniBible.Tests;

public static class AssertEx
{
    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"Expected {expected}, got {actual}");
    }

    public static void True(bool value, string message = "Expected true")
    {
        if (!value) throw new Exception(message);
    }

    public static T Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T ex) { return ex; }
        catch (Exception ex) { throw new Exception($"Expected {typeof(T).Name}, got {ex.GetType().Name}"); }
        throw new Exception($"Expected {typeof(T).Name}, but no exception was thrown");
    }
}
