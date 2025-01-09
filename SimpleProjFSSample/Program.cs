using System.ComponentModel;
using CommandLine;
using SimpleProjFSSample;

CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(MainEntry);
return s_exitCode;


public static partial class Program
{
    private static int s_exitCode = 0;

    private static int? s_buildNumber;
    public static int BuildNumber => s_buildNumber ??= GetWindowsBuildNumber();

    private static int GetWindowsBuildNumber()
    {
        int build = Convert.ToInt32(Microsoft.Win32.Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            "CurrentBuild",
            0
        ));
        Logger.Info($"windows build number: {build}");
        return build;
    }

    private static void MainEntry(CommandLineOptions opts)
    {
        Logger.Level fileLoggingLevel = opts.Debug ? Logger.Level.Debug : Logger.Level.Info;
        Logger.Initialize(fileLoggingLevel, consoleLoggingLevel: Logger.Level.Error);
        Logger.Info("start");

        try {
            if (CheckAndEnableProjFS() && InitializeProjFSApi()) {
                RunSimpleProvider(opts);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.Write("Press [Enter] to exit...");
            Console.ReadLine();
            Logger.Info($"exit {s_exitCode}");
        }
        catch (Exception ex) {
            Logger.Fatal($"caught an unexpected exception: {Environment.NewLine}{ex}");
            s_exitCode = 1;
        }
        finally {
            Logger.Dispose();
        }
    }

    private static bool CheckAndEnableProjFS()
    {
        string? line;
        if (ProjFSFeatureController.CheckProjFSEnabled()) {
            Console.WriteLine("The ProjFS feature is enabled.");
            return true;
        }
        else if (BuildNumber < nyasProjFS.BuildNumbers.Minimal) {
            Console.WriteLine("The ProjFS feature is available on windows 10 version 1803 and above, please upgrade your windows.");
            return false;
        }

        // using ProjFSFeatureController.exe to enable feature should be a optional Functionality
        if (!File.Exists(ProjFSFeatureController.ExePath)) {
            Logger.Info("could not find ProjFSFeatureController.exe");
            goto ReturnFalse;
        }

        Console.WriteLine("The ProjFS feature is not enabled, Do you want to enable ProjFS now? (administrator required) [y/n, default:n]");
        line = Console.ReadLine();
        Logger.Debug($"user input: {line}");

        if (line is not null && char.ToLower(line[0]) == 'y') {
            try {
                ProjFSFeatureController.Exe(enableFeature: true);

                if (ProjFSFeatureController.CheckProjFSEnabled()) {
                    Console.WriteLine("The ProjFS feature has been successfully enabled.");
                    return true;
                }
                else {
                    Console.WriteLine("Failed to enable the ProjFS feature.");
                    goto ReturnFalse;
                }
            }
            catch (NotSupportedException ex) {
                Logger.Error(" -- " + ex.Message);
                goto ReturnFalse;
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223) {
                Logger.Info(" -- running ProjFSFeatureController.exe was canceled by the user");
            }
        }
        Console.WriteLine("This program need ProjFS to run.");

        ReturnFalse:
        Console.WriteLine();
        Console.WriteLine("You can manually enter `Enable-WindowsOptionalFeature -Online -FeatureName Client-ProjFS -NoRestart` in Windows PowerShell to enable ProjFS.");
        Console.WriteLine("If ProjFS is already enabled and you still see this message, please contact the author on github.");
        return false;
    }

    private static bool InitializeProjFSApi()
    {
        try {
            nyasProjFS.ApiHelper.Initialize();
            Logger.Info("the ProjFS API has been successfully initialized");
            Logger.Debug($"the ProjFS API has windows build number {nyasProjFS.ApiHelper.ApiLevel}");
            return true;
        }
        catch (DllNotFoundException ex) {
            Logger.Error($" -- could not find dll: \n{ex.Message}");
        }
        catch (BadImageFormatException ex) {
            Logger.Error($" -- could not load dll: \n{ex.Message}");
        }
        catch (EntryPointNotFoundException ex) {
            Logger.Error($" -- could not find entry point: \n{ex.Message}");
        }
        return false;
    }

    private static void RunSimpleProvider(CommandLineOptions opts)
    {
        SimpleProvider provider;
        try {
            provider = new(opts);
        }
        catch (Exception) {
            Logger.Fatal("Failed to create SimpleProvider.");
            throw;
        }

        Logger.Info("Starting provider");

        if (!provider.StartVirtualization()) {
            Logger.Error("Could not start provider.");
            s_exitCode = 1;
            return;
        }
        Console.WriteLine("Provider is running.");
    }
}