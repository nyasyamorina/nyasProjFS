using System.ComponentModel;
using CommandLine;
using SimpleProjFSSample;

static class Program
{
    private static int s_exitCode = 0;

    public static int Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(MainEntry);
        return s_exitCode;
    }

    private static void MainEntry(CommandLineOptions opts)
    {
        Logger.Level fileLoggingLevel = opts.Debug ? Logger.Level.Debug : Logger.Level.Info;
        Logger.Initialize(consoleLoggingLevel: Logger.Level.Warn, opts);
        Logger.Info("start");

        SimpleProvider? provider = null;
        try {
            if (EnvironmentHelper.CheckAndEnableProjFS() && InitializeProjFSApi()) {
                provider = SimpleProvider.Run(opts);
                if (provider is null) { s_exitCode = 1; }
                else { Console.WriteLine("Provider is running."); }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.Write("Press [Enter] to exit...");
            Console.ReadLine();

            provider?.StopVirtualization();
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
        catch (EntryPointNotFoundException) {
            Logger.Error($" -- could not find ProjFS entry point");
        }
        return false;
    }
}