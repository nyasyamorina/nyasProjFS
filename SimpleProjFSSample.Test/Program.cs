using System.Diagnostics;

namespace SimpleProjFSSample.Test;

class Program
{
    private static int? s_buildNumber;
    public static int BuildNumber => s_buildNumber ??= GetWindowsBuildNumber();

    private static int GetWindowsBuildNumber()
    {
        int build = Convert.ToInt32(Microsoft.Win32.Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            "CurrentBuild",
            0
        ));
        return build;
    }

    public static void Main(string[] args)
    {
        NUnitRunner runner = new(args);
        List<string> excludeCategories = [];

        if (BuildNumber < 19041) {
            excludeCategories.Add(BasicTests.SymlinkTestCategory);
        }

        Environment.ExitCode = runner.RunTests(includeCategories: [], excludeCategories: excludeCategories);

        if (Debugger.IsAttached) {
            Console.WriteLine("Tests completed. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}