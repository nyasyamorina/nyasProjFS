using System.Diagnostics;
using CommandLine;
using static FeatureOperations;

Parser.Default.ParseArguments<ControllerOptions>(args).WithParsed(opts =>
{
    int exitCode = 0;
    bool restartNeeded = false;

    if (opts.Enable && opts.Disable) { // cannot be enabled and disabled at the same time
        exitCode = 1;
    }
    else if (opts.Enable) {
        exitCode = EnableOptional("Client-ProjFS", out restartNeeded);
    }
    else if (opts.Disable) {
        exitCode = DisableOptional("Client-ProjFS", out restartNeeded);
    }

    if (restartNeeded) { Console.WriteLine("Need to Restart computer to apply the changes"); }
    Environment.Exit(exitCode);
});

class ControllerOptions
{
    [Option('e', "enable", HelpText = "enable windows optional feature Client-ProjFS")]
    public bool Enable { get; set; }
    [Option('d', "disable", HelpText = "disable windows optional feature Client-ProjFS")]
    public bool Disable { get; set; }
}

static class FeatureOperations
{
    internal static int EnableOptional(string featureName, out bool restartNeeded)
    {
        string command = $"Enable-WindowsOptionalFeature -Online -FeatureName {featureName} -NoRestart";
        return RunInWindowsPowerShellAndCheckRestartNeeded(command, out restartNeeded);
    }
    internal static int DisableOptional(string featureName, out bool restartNeeded)
    {
        string command = $"Disable-WindowsOptionalFeature -Online -FeatureName {featureName} -NoRestart";
        return RunInWindowsPowerShellAndCheckRestartNeeded(command, out restartNeeded);
    }

    private static int RunInWindowsPowerShellAndCheckRestartNeeded(string command, out bool restartNeeded)
    {
        restartNeeded = false;

        string System32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
        string psPath = Path.Join(System32, "WindowsPowerShell", "v1.0", "powershell.exe");
        if (!File.Exists(psPath)) { throw new FileNotFoundException("not found powershell in system", psPath); }

        using Process ps = new();
        ps.StartInfo.FileName = psPath;
        ps.StartInfo.Arguments = $"-NoProfile -ExecutionPolicy Unrestricted -Command \"& {{{command}}}\"";
        ps.StartInfo.UseShellExecute = false;
        ps.StartInfo.RedirectStandardOutput = true;
        ps.Start();
        ps.WaitForExit();

        string? line;
        StreamReader reader = ps.StandardOutput;
        while ((line = reader.ReadLine()) is not null) {
            if (line.Contains("RestartNeeded")) {
                if (line.Contains("True")) { restartNeeded = true; }
                break;
            }
        }
        return ps.ExitCode;
    }
}