using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataTool.Core.Logging;
using DataTool.Core.Provinces;
using DataTool.Core.Tasks;
using MaterialDesignThemes.Wpf;

namespace DataTool.App.Models
{
    // TẠM THỜI cho giai đoạn dựng giao diện.
    // Ở bước VBDLIS, mô tả tác vụ sẽ lấy từ các class implement ITask (TaskRegistry) và chạy bằng TaskRunner.

    /// <summary>Thông tin một tác vụ hiển thị trên menu.</summary>
    public class TaskDescriptor
    {
        /// <summary>Đường dẫn menu, phân cấp bằng dấu "/". Ví dụ "VBDLIS/Publish code".</summary>
        public string MenuPath { get; set; }
        public string Description { get; set; }
        public bool IsDangerous { get; set; }
        public PackIconKind Icon { get; set; } = PackIconKind.PlayCircleOutline;

        /// <summary>Tham số người dùng nhập trước khi chạy; màn hình chạy tự sinh form.</summary>
        public IReadOnlyList<TaskParameter> Parameters { get; set; } = Array.Empty<TaskParameter>();

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

    /// <summary>Ngữ cảnh truyền cho tác vụ giả lập: tỉnh, báo tiến trình, ghi log, huỷ.</summary>
    public class FakeTaskContext
    {
        private readonly IProgress<ProgressInfo> _progress;
        private readonly IProgress<LogEntry> _log;

        public FakeTaskContext(ProvinceProfile province, IProgress<ProgressInfo> progress,
            IProgress<LogEntry> log, CancellationToken cancellationToken,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            Province = province;
            Parameters = parameters ?? new Dictionary<string, string>();
            _progress = progress;
            _log = log;
            CancellationToken = cancellationToken;
        }

        public ProvinceProfile Province { get; }
        public CancellationToken CancellationToken { get; }
        public IReadOnlyDictionary<string, string> Parameters { get; }

        public string GetParameter(string key) => Parameters.TryGetValue(key, out var v) ? v : null;
        public IReadOnlyList<string> GetCodeList(string key) => CodeListParser.Parse(GetParameter(key));

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
}
