using CommandLine;
using CommandLine.Text;

namespace SimpleProjFSSample;

public sealed class CommandLineOptions
{
#if DEBUG
    const bool required = false;
#else
    const bool required = true;
#endif

    [Option("debug", Hidden = true, HelpText = "output debug message to log file")]
    public bool Debug { get; set; } = false;

    [Option(Required = required, HelpText = "Path to the files and directories to project.")]
    public string SourceRoot { get; set; } = "";

    [Option(Required = required, HelpText = "Path to the virtualization root.")]
    public string VirtRoot { get; set; } = "";

    [Option('t', "testmode", HelpText = "Use this when running the provider with the test package.", Hidden = true)]
    public bool TestMode { get; set; } = false;

    [Option('n', "notifications", HelpText = "Enable file system operation notifications.")]
    public bool EnableNotifications { get; set; } = false;

    [Option('d', "denyDeletes", HelpText = "Deny deletes.", Hidden = true)]
    public bool DenyDeletes { get; set; } = false;

    [Usage(ApplicationAlias = "SimpleProviderManaged")]
    public static IEnumerable<Example> Examples
    {
        get
        {
            return [
                new(
                    "Start provider, projecting files and directories from 'c:\\source' into 'c:\\virtRoot'",
                    new CommandLineOptions { SourceRoot = "c:\\source", VirtRoot = "c:\\virtRoot" }
                )
            ];
        }
    }
}