using System.Diagnostics;
using System.Management;

namespace SimpleProjFSSample;

public static class ProjFSFeatureController
{
    /// <summary>the path of program ProjFSFeatureController.exe</summary>
    public static string ExePath { get; } = Path.Join(AppContext.BaseDirectory, "ProjFSFeatureController.exe");

    /// <summary>run the program ProjFSFeatureController.exe</summary>
    /// <exception cref="NotSupportedException">only Windows Vista and later supports privilege escalation</exception>
    /// <exception cref="FileNotFoundException"></exception>
    public static void Exe(bool enableFeature = false, bool disableFeature = false)
    {
        Logger.Debug(" -> running ProjFSFeatureController.exe");

        if (Environment.OSVersion.Version.Major < 6) { throw new NotSupportedException("only Windows Vista and later supports privilege escalation"); }
        if (!File.Exists(ExePath)) { throw new FileNotFoundException("cannot found ProjFSFeatureController.exe", ExePath); }

        using Process controller = new();
        controller.StartInfo.FileName = ExePath;
        controller.StartInfo.Arguments = (enableFeature ? "--enable " : "") + (disableFeature ? "--disable" : "");
        controller.StartInfo.UseShellExecute = true;
        controller.StartInfo.Verb = "runas"; // visita and later
        controller.Start();
        controller.WaitForExit();
        Logger.Debug($" <- ProjFSFeatureController.exe exit with exit code {controller.ExitCode}");
    }

    /// <summary>check windows feature ProjFS is enabled or not without running ProjFSFeatureController.exe</summary>
    public static bool CheckProjFSEnabled()
    {
        Logger.Debug(" -> checking ProjFS is enabled or not");

        const string query = "Select * From Win32_OptionalFeature Where Caption Like '%Windows Projected File System%'";

        bool enabled = false;
        try {
            using ManagementObjectSearcher searcher = new(query);
            using ManagementObjectCollection mObjs = searcher.Get();

            using ManagementObjectCollection.ManagementObjectEnumerator enumerator = mObjs.GetEnumerator();
            if (enumerator.MoveNext()) { // should be no more than one
                using ManagementBaseObject mBaseObj = enumerator.Current;
                enabled = mBaseObj.Properties["InstallState"].Value.ToString() == "1";
            }
        }
        catch (ManagementException ex) when (ex.ErrorCode == ManagementStatus.NotFound) {
            Logger.Debug("could not found windows feature \"Windows Projected File System\" in win32");
        }

        Logger.Debug($" <- checked ProjFS enabled: {enabled}");
        return enabled;
    }
}