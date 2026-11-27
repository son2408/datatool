using System;
using System.Collections.Generic;
using System.Threading;
using DataTool.Core.Provinces;
using DataTool.Core.Logging;

namespace DataTool.Core.Tasks
{
    public sealed class TaskProgress
    {
        public TaskProgress(double percent, string message)
        {
            Percent = percent < 0 ? 0 : percent > 100 ? 100 : percent;
            Message = message;
        }

        /// <summary>0 đến 100; giá trị âm truyền vào được coi là 0.</summary>
        public double Percent { get; }
        public string Message { get; }
    }

    /// <summary>Những gì một tác vụ nhận được khi chạy.</summary>
    public interface ITaskContext
    {
        /// <summary>Cấu hình của tỉnh đang chọn.</summary>
        ProvinceProfile Province { get; }
        CancellationToken CancellationToken { get; }
        ITaskLogger Logger { get; }

        /// <summary>Tham số thêm (ví dụ từ dòng lệnh ConsoleDemo). Không phân biệt hoa thường.</summary>
        IReadOnlyDictionary<string, string> Parameters { get; }

        void ReportProgress(double percent, string message = null);
    }

    public static class TaskContextExtensions
    {
        public static void Log(this ITaskContext ctx, LogLevel level, string message) => ctx.Logger.Log(level, message);
        public static void Info(this ITaskContext ctx, string message) => ctx.Logger.Info(message);
        public static void Warning(this ITaskContext ctx, string message) => ctx.Logger.Warning(message);
        public static void Error(this ITaskContext ctx, string message) => ctx.Logger.Error(message);
        public static void Success(this ITaskContext ctx, string message) => ctx.Logger.Success(message);
        public static void ThrowIfCancelled(this ITaskContext ctx) => ctx.CancellationToken.ThrowIfCancellationRequested();

        public static string GetParameter(this ITaskContext ctx, string name, string defaultValue = null)
            => ctx.Parameters != null && ctx.Parameters.TryGetValue(name, out var v) ? v : defaultValue;
    }

    internal sealed class TaskContext : ITaskContext
    {
        private readonly IProgress<TaskProgress> _progress;

        public TaskContext(ProvinceProfile province, ITaskLogger logger, IProgress<TaskProgress> progress,
            IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken)
        {
            Province = province ?? throw new ArgumentNullException(nameof(province));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _progress = progress;
            Parameters = parameters ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            CancellationToken = cancellationToken;
        }

        public ProvinceProfile Province { get; }
        public CancellationToken CancellationToken { get; }
        public ITaskLogger Logger { get; }
        public IReadOnlyDictionary<string, string> Parameters { get; }

        public void ReportProgress(double percent, string message = null) => _progress?.Report(new TaskProgress(percent, message));
    }
}
