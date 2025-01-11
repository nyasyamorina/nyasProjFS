using System.Diagnostics;

namespace SimpleProjFSSample.Test;

class Program
{
    public static void Main(string[] args)
    {
        if (Directory.Exists("Log")) { Directory.Delete("Log", true); }
        CommandLineOptions opts = new() {
            LogPath = Path.Join("Log", "SimpleProjFSSample.Test.log"),
            LogLevel = "Debug",
        };
        Logger.Initialize(Logger.Level.Fatal, opts);

        if (!EnvironmentHelper.CheckAndEnableProjFS()) {
            throw new InvalidOperationException("ProjFS is not enabled.");
        }

        NUnitRunner runner = new(args);
        List<string> excludeCategories = [];

        if (!EnvironmentHelper.IsSymlinkSupportAvailable) {
            excludeCategories.Add(BasicTests.SymlinkTestCategory);
        }

        Environment.ExitCode = runner.RunTests(includeCategories: [], excludeCategories: excludeCategories);

        if (Debugger.IsAttached) {
            Console.WriteLine("Tests completed. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}