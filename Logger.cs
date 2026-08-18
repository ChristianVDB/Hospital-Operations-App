using System;
using System.IO;
using System.Text;
using System.Threading;

namespace HospitalOperationsSystem
{
    public enum LogLevel { Trace = 0, Debug = 1, Info = 2, Warn = 3, Error = 4, Critical = 5 }

    public static class Logger
    {
        private static readonly object _sync = new();
        private static StreamWriter? _writer;
        private static LogLevel _minLevel = LogLevel.Info;
        private static string? _logDirectory;

        public static void Init(string logDirectory, LogLevel minLevel = LogLevel.Info)
        {
            lock (_sync)
            {
                _minLevel = minLevel;
                _logDirectory = logDirectory;
                try
                {
                    Directory.CreateDirectory(logDirectory);
                    string filePath = Path.Combine(logDirectory, $"app-{DateTime.UtcNow:yyyyMMdd}.log");
                    _writer = new StreamWriter(new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    {
                        AutoFlush = true
                    };
                    LogInternal(LogLevel.Info, "Logger initialized", null, writeToConsole: false);
                }
                catch
                {
                    // If file logging can't be initialized, we still allow console logging
                    _writer = null;
                }
            }
        }

        private static void LogInternal(LogLevel level, string message, Exception? ex = null, bool writeToConsole = true)
        {
            if (level < _minLevel) return;

            string time = DateTime.UtcNow.ToString("o");
            int threadId = Thread.CurrentThread.ManagedThreadId;
            var sb = new StringBuilder();
            sb.Append($"[{time}] [{threadId}] [{level}] {message}");
            if (ex != null) sb.Append($" | EX: {ex.GetType().Name}: {ex.Message} {ex.StackTrace}");
            string line = sb.ToString();

            if (writeToConsole)
            {
                try
                {
                    ConsoleColor original = Console.ForegroundColor;
                    Console.ForegroundColor = level switch
                    {
                        LogLevel.Trace => ConsoleColor.DarkGray,
                        LogLevel.Debug => ConsoleColor.Gray,
                        LogLevel.Info => ConsoleColor.Green,
                        LogLevel.Warn => ConsoleColor.Yellow,
                        LogLevel.Error => ConsoleColor.Red,
                        LogLevel.Critical => ConsoleColor.Magenta,
                        _ => ConsoleColor.White
                    };
                    Console.WriteLine(line);
                    Console.ForegroundColor = original;
                }
                catch { }
            }

            try
            {
                lock (_sync)
                {
                    _writer?.WriteLine(line);
                }
            }
            catch { }
        }

        public static void Trace(string message) => LogInternal(LogLevel.Trace, message);
        public static void Debug(string message) => LogInternal(LogLevel.Debug, message);
        public static void Info(string message) => LogInternal(LogLevel.Info, message);
        public static void Warn(string message) => LogInternal(LogLevel.Warn, message);
        public static void Error(string message, Exception? ex = null) => LogInternal(LogLevel.Error, message, ex);
        public static void Critical(string message, Exception? ex = null) => LogInternal(LogLevel.Critical, message, ex);

        public static void Shutdown()
        {
            try
            {
                lock (_sync)
                {
                    _writer?.Flush();
                    _writer?.Dispose();
                    _writer = null;
                }
            }
            catch { }
        }
    }
}
