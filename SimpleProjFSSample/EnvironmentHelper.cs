using System.ComponentModel;

namespace SimpleProjFSSample;

public static class EnvironmentHelper
{
    private static int? s_buildNumber;
    public static int BuildNumber => s_buildNumber ??= GetWindowsBuildNumber();

    public static bool IsSymlinkSupportAvailable { get; } = BuildNumber >= 19041;

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



    public static bool CheckAndEnableProjFS()
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
}