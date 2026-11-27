using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataTool.Core.Logging
{
    public enum LogLevel
    {
        Debug,
        Info,
        Success,
        Warning,
        Error
    }

    public sealed class LogEntry
    {
        public LogEntry(LogLevel level, string message, DateTime? timestamp = null)
        {
            Level = level;
            Message = message ?? string.Empty;
            Timestamp = timestamp ?? DateTime.Now;
        }

        public DateTime Timestamp { get; }
        public LogLevel Level { get; }
        public string Message { get; }

        public override string ToString() => $"[{Timestamp:HH:mm:ss}] {Level,-7} {Message}";
    }

    public interface ITaskLogger
    {
        void Log(LogLevel level, string message);
    }

    public static class TaskLoggerExtensions
    {
        public static void Debug(this ITaskLogger logger, string message) => logger.Log(LogLevel.Debug, message);
        public static void Info(this ITaskLogger logger, string message) => logger.Log(LogLevel.Info, message);
        public static void Success(this ITaskLogger logger, string message) => logger.Log(LogLevel.Success, message);
        public static void Warning(this ITaskLogger logger, string message) => logger.Log(LogLevel.Warning, message);
        public static void Error(this ITaskLogger logger, string message) => logger.Log(LogLevel.Error, message);
        public static void Error(this ITaskLogger logger, Exception ex, string message = null)
            => logger.Log(LogLevel.Error, string.IsNullOrEmpty(message) ? ex.ToString() : message + Environment.NewLine + ex);
    }

    /// <summary>Logger chuyển mỗi dòng cho một hàm (ví dụ đẩy lên giao diện qua IProgress).</summary>
    public sealed class DelegateLogger : ITaskLogger
    {
        private readonly Action<LogEntry> _write;
        public DelegateLogger(Action<LogEntry> write) => _write = write ?? throw new ArgumentNullException(nameof(write));
        public void Log(LogLevel level, string message) => _write(new LogEntry(level, message));
    }

    /// <summary>Ghi cùng lúc ra nhiều logger.</summary>
    public sealed class CompositeLogger : ITaskLogger
    {
        private readonly ITaskLogger[] _loggers;
        public CompositeLogger(IEnumerable<ITaskLogger> loggers) => _loggers = loggers.Where(l => l != null).ToArray();
        public CompositeLogger(params ITaskLogger[] loggers) : this((IEnumerable<ITaskLogger>)loggers) { }

        public void Log(LogLevel level, string message)
        {
            foreach (var logger in _loggers) logger.Log(level, message);
        }
    }

    /// <summary>Ghi log ra file văn bản UTF-8, an toàn khi gọi từ nhiều luồng.</summary>
    public sealed class FileLogger : ITaskLogger, IDisposable
    {
        private readonly object _sync = new object();
        private readonly StreamWriter _writer;

        public FileLogger(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath)));
            _writer = new StreamWriter(new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read), new UTF8Encoding(false))
            {
                AutoFlush = true
            };
        }

        public string FilePath { get; }

        /// <summary>Tạo file log mới trong thư mục, tên dạng 20261127_101530_Ten-tac-vu.log.</summary>
        public static FileLogger CreateForRun(string folder, string taskTitle)
        {
            string safe = new string((taskTitle ?? "task").Select(c => Path.GetInvalidFileNameChars().Contains(c) || c == ' ' ? '-' : c).ToArray());
            if (safe.Length > 60) safe = safe.Substring(0, 60);
            string path = Path.Combine(folder, $"{DateTime.Now:yyyyMMdd_HHmmss}_{safe}.log");
            return new FileLogger(path);
        }

        public void Log(LogLevel level, string message)
        {
            lock (_sync)
            {
                _writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level.ToString().ToUpperInvariant(),-7}] {message}");
            }
        }

        public void Dispose()
        {
            lock (_sync) _writer.Dispose();
        }
    }
}
