using CommandLine;

namespace SimpleProjFSSample;

public sealed class CommandLineOptions
{
    [Option("debug", Hidden = true, HelpText = "output debug message to log file")]
    public bool Debug { get; set; } = false;
}