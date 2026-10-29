namespace MiniBible.App;

static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        if (args.Contains("--smoke-test")) return 0;
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
        return 0;
    }
}
