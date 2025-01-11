namespace SimpleProjFSSample;

public static class Logger
{
    public enum Level : int {
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
        Close,
    }

    private static string GetLoggingString(Level level, string message)
    {
        return $"[{level}][{DateTime.Now:yyMMdd-HHmmss}] {message}";
    }

    private static readonly Lock s_loggerLock = new();
    private static bool s_initialized = false;
    private static FileStream? s_logFileStream;
    private static StreamWriter? s_logFileWriter;

    private static void NoLogging(string _) { }
    private static Action<string> s_logToFile = NoLogging;
    private static Action<string> s_logToConsole = NoLogging;

    private static Level s_fileLoggingLevel = Level.Info;
    private static Action<string> s_logDebugToFile = NoLogging;
    private static Action<string> s_logInfoToFile = NoLogging;
    private static Action<string> s_logWarnToFile = NoLogging;
    private static Action<string> s_logErrorToFile = NoLogging;
    private static Action<string> s_logFatalToFile = NoLogging;

    private static Level s_consoleLoggingLevel = Level.Info;
    private static Action<string> s_logDebugToConsole = NoLogging;
    private static Action<string> s_logInfoToConsole = NoLogging;
    private static Action<string> s_logWarnToConsole = NoLogging;
    private static Action<string> s_logErrorToConsole = NoLogging;
    private static Action<string> s_logFatalToConsole = NoLogging;

    public static Level FileLoggingLevel {
        get => s_fileLoggingLevel;
        set
        {
            s_fileLoggingLevel = value;
            if (s_initialized) {
                s_logDebugToFile = Level.Debug >= value ? s_logToFile : NoLogging;
                s_logInfoToFile = Level.Info >= value ? s_logToFile : NoLogging;
                s_logWarnToFile = Level.Warn >= value ? s_logToFile : NoLogging;
                s_logErrorToFile = Level.Error >= value ? s_logToFile : NoLogging;
                s_logFatalToFile = Level.Fatal >= value ? s_logToFile : NoLogging;
            }
        }
    }
    public static Level ConsoleLoggingLevel {
        get => s_consoleLoggingLevel;
        set
        {
            s_consoleLoggingLevel = value;
            if (s_initialized) {
                s_logDebugToConsole = Level.Debug >= value ? s_logToConsole : NoLogging;
                s_logInfoToConsole = Level.Info >= value ? s_logToConsole : NoLogging;
                s_logWarnToConsole = Level.Warn >= value ? s_logToConsole : NoLogging;
                s_logErrorToConsole = Level.Error >= value ? s_logToConsole : NoLogging;
                s_logFatalToConsole = Level.Fatal >= value ? s_logToConsole : NoLogging;
            }
        }
    }

    public static void Initialize(Level consoleLoggingLevel, CommandLineOptions opts)
    {
        lock (s_loggerLock) {
            string logPath;
            if (opts.LogPath is null) {
                DateTime startTime = DateTime.Now;
                Guid loggerFileGuid = Guid.NewGuid();
                logPath = Path.Combine(AppContext.BaseDirectory, "Log", $"{startTime:yyMMdd-HHmmss}+{loggerFileGuid}.log");
            }
            else { logPath = opts.LogPath; }

            string? logDir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir)) { Directory.CreateDirectory(logDir); }

            s_logFileStream = new(logPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            s_logFileWriter = new(s_logFileStream) {
                AutoFlush = false
            };

            s_logToFile = message =>
            {
                s_logFileWriter.WriteLine(message);
                s_logFileWriter.Flush();
                s_logFileStream.Flush();
            };
            s_logToConsole = Console.WriteLine;

            Level fileLoggingLevel = Level.Info;
            bool getLevelFromOpts = true;
            if (!string.IsNullOrEmpty(opts.LogLevel)) {
                getLevelFromOpts = false;
                foreach (Level level in Enum.GetValues<Level>()) {
                    if (opts.LogLevel.Equals(level.ToString(), StringComparison.InvariantCultureIgnoreCase)) {
                        fileLoggingLevel = level;
                        getLevelFromOpts = true;
                    }
                }
            }

            s_initialized = true;
            FileLoggingLevel = fileLoggingLevel;
            ConsoleLoggingLevel = consoleLoggingLevel;

            if (!getLevelFromOpts) {
                Logger.Warn($"unknown log level: {opts.LogLevel}, please choose from [{string.Join(',', Enum.GetNames<Level>())}]");
            }
        }
    }
    public static void Dispose()
    {
        lock (s_loggerLock) {
            s_initialized = false;
            FileLoggingLevel = Level.Close;
            ConsoleLoggingLevel = Level.Close;

            s_logToFile = NoLogging;
            s_logToConsole = NoLogging;

            s_logFileStream?.Close();
            s_logFileWriter = null;
            s_logFileStream = null;
        }
    }

    public static void Log(Level level, string message)
    {
        string str = GetLoggingString(level, message);
        lock (s_loggerLock) switch (level) {
            case Level.Debug:
                s_logDebugToFile(str);
                s_logDebugToConsole(str);
                break;
            case Level.Info:
                s_logInfoToFile(str);
                s_logInfoToConsole(str);
                break;
            case Level.Warn:
                s_logWarnToFile(str);
                s_logWarnToConsole(str);
                break;
            case Level.Error:
                s_logErrorToFile(str);
                s_logErrorToConsole(str);
                break;
            case Level.Fatal:
                s_logFatalToFile(str);
                s_logFatalToConsole(str);
                break;
            case Level.Close:
                break;
        }
    }

    public static void Debug(string message)
    {
        lock (s_loggerLock) {
            string str = GetLoggingString(Level.Debug, message);
            s_logDebugToFile(str);
            s_logDebugToConsole(str);
        }
    }
    public static void Info(string message)
    {
        lock (s_loggerLock) {
            string str = GetLoggingString(Level.Info, message);
            s_logInfoToFile(str);
            s_logInfoToConsole(str);
        }
    }
    public static void Warn(string message)
    {
        lock (s_loggerLock) {
            string str = GetLoggingString(Level.Warn, message);
            s_logWarnToFile(str);
            s_logWarnToConsole(str);
        }
    }
    public static void Error(string message)
    {
        lock (s_loggerLock) {
            string str = GetLoggingString(Level.Error, message);
            s_logErrorToFile(str);
            s_logErrorToConsole(str);
        }
    }
    public static void Fatal(string message)
    {
        lock (s_loggerLock) {
            string str = GetLoggingString(Level.Fatal, message);
            s_logFatalToFile(message);
            s_logFatalToConsole(message);
        }
    }
}