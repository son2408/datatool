using System;
using System.Threading;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;

namespace DataTool.App.Models
{
    // TẠM THỜI cho giai đoạn dựng giao diện.
    // Ở bước Core, mô tả tác vụ sẽ lấy từ các class implement ITask (quét bằng reflection).

    /// <summary>Thông tin một tác vụ hiển thị trên menu.</summary>
    public class TaskDescriptor
    {
        /// <summary>Đường dẫn menu, phân cấp bằng dấu "/". Ví dụ "VBDLIS/Publish code".</summary>
        public string MenuPath { get; set; }
        public string Description { get; set; }
        public bool IsDangerous { get; set; }
        public PackIconKind Icon { get; set; } = PackIconKind.PlayCircleOutline;

        /// <summary>Phần xử lý (đang giả lập).</summary>
        public Func<FakeTaskContext, Task> Execute { get; set; }

        public string Title
        {
            get
            {
                int index = MenuPath?.LastIndexOf('/') ?? -1;
                return index >= 0 ? MenuPath.Substring(index + 1) : MenuPath;
            }
        }

        public string Group
        {
            get
            {
                int index = MenuPath?.LastIndexOf('/') ?? -1;
                return index > 0 ? MenuPath.Substring(0, index).Replace("/", " › ") : string.Empty;
            }
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>Ngữ cảnh truyền cho tác vụ giả lập: môi trường, báo tiến trình, ghi log, huỷ.</summary>
    public class FakeTaskContext
    {
        private readonly IProgress<ProgressInfo> _progress;
        private readonly IProgress<LogEntry> _log;

        public FakeTaskContext(EnvironmentProfile environment, IProgress<ProgressInfo> progress,
            IProgress<LogEntry> log, CancellationToken cancellationToken)
        {
            Environment = environment;
            _progress = progress;
            _log = log;
            CancellationToken = cancellationToken;
        }

        public EnvironmentProfile Environment { get; }
        public CancellationToken CancellationToken { get; }

        public void Report(double percent, string message) => _progress.Report(new ProgressInfo(percent, message));
        public void Log(LogLevel level, string message) => _log.Report(new LogEntry(level, message));
        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warn(string message) => Log(LogLevel.Warning, message);
    }

    public sealed class ProgressInfo
    {
        public ProgressInfo(double percent, string message)
        {
            Percent = percent;
            Message = message;
        }

        public double Percent { get; }
        public string Message { get; }
    }

    public sealed class LogEntry
    {
        public LogEntry(LogLevel level, string message)
        {
            Timestamp = DateTime.Now;
            Level = level;
            Message = message;
        }

        public DateTime Timestamp { get; }
        public LogLevel Level { get; }
        public string Message { get; }

        public override string ToString() => $"[{Timestamp:HH:mm:ss}] {Level,-7} {Message}";
    }
}
