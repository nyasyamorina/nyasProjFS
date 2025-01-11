using CommandLine;
using CommandLine.Text;

namespace SimpleProjFSSample;

public sealed class CommandLineOptions
{
    [Option("debug", Hidden = true, HelpText = "output debug message to log file")]
    public bool Debug { get; set; } = false;

    [Option(Required = true, HelpText = "Path to the files and directories to project.")]
    public string SourceRoot { get; set; } = "";

    [Option(Required = true, HelpText = "Path to the virtualization root.")]
    public string VirtRoot { get; set; } = "";

    [Option(HelpText = "Set the path of log output file.")]
    public string? LogPath { get; set; }

    [Option(HelpText = "Set the log level of log output file")]
    public string? LogLevel { get; set; }

    [Option('t', "testmode", HelpText = "Use this when running the provider with the test package.", Hidden = true)]
    public bool TestMode { get; set; } = false;

    [Option('n', "notifications", HelpText = "Enable file system operation notifications.")]
    public bool EnableNotifications { get; set; } = false;

    [Option('d', "denyDeletes", HelpText = "Deny deletes.", Hidden = true)]
    public bool DenyDeletes { get; set; } = false;

    [Usage(ApplicationAlias = "SimpleProjFSSample")]
    public static IEnumerable<Example> Examples => [ new(
        "Start provider, projecting files and directories from 'c:\\source' into 'c:\\virtRoot'",
        new CommandLineOptions { SourceRoot = "c:\\source", VirtRoot = "c:\\virtRoot" }
    ) ];
}