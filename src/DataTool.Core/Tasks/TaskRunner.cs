using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DataTool.Core.Provinces;
using DataTool.Core.Logging;

namespace DataTool.Core.Tasks
{
    public enum TaskRunStatus
    {
        Succeeded,
        Cancelled,
        Failed
    }

    public sealed class TaskRunResult
    {
        public TaskRunResult(TaskRunStatus status, TimeSpan duration, Exception error, string logFilePath)
        {
            Status = status;
            Duration = duration;
            Error = error;
            LogFilePath = logFilePath;
        }

        public TaskRunStatus Status { get; }
        public TimeSpan Duration { get; }
        public Exception Error { get; }
        public string LogFilePath { get; }
    }

    public sealed class TaskRunOptions
    {
        /// <summary>Thư mục ghi file log; null thì không ghi file.</summary>
        public string LogFolder { get; set; }

        /// <summary>Nhận từng dòng log (giao diện, console).</summary>
        public ITaskLogger Logger { get; set; }

        public IProgress<TaskProgress> Progress { get; set; }

        public IReadOnlyDictionary<string, string> Parameters { get; set; }
    }

    /// <summary>
    /// Chạy một ITask trên luồng nền: ghi log ra file, bắt lỗi, xử lý huỷ, đo thời gian.
    /// Không biết gì về giao diện; dùng chung cho App và ConsoleDemo.
    /// Việc xác nhận (tác vụ xoá phải gõ lại mã tỉnh) do lớp giao diện quyết định trước khi gọi.
    /// </summary>
    public class TaskRunner
    {
        public async Task<TaskRunResult> RunAsync(ITask task, ProvinceProfile province,
            TaskRunOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (province == null) throw new ArgumentNullException(nameof(province));
            options = options ?? new TaskRunOptions();

            string title = TaskPath.GetTitle(task.MenuPath);
            FileLogger fileLogger = null;
            var loggers = new List<ITaskLogger>();
            if (options.Logger != null) loggers.Add(options.Logger);

            if (!string.IsNullOrEmpty(options.LogFolder))
            {
                try
                {
                    fileLogger = FileLogger.CreateForRun(options.LogFolder, title);
                    loggers.Add(fileLogger);
                }
                catch (Exception ex)
                {
                    options.Logger?.Warning("Không tạo được file log: " + ex.Message);
                }
            }

            var logger = new CompositeLogger(loggers);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                logger.Info($"Bắt đầu \"{title}\" trên tỉnh {province.DisplayName} ({province.Id}).");
                if (fileLogger != null) logger.Debug("File log: " + fileLogger.FilePath);

                var context = new TaskContext(province, logger, options.Progress, options.Parameters, cancellationToken);

                // Task.Run để code đồng bộ trong tác vụ không làm treo giao diện.
                await Task.Run(() => task.ExecuteAsync(context), cancellationToken).ConfigureAwait(false);

                stopwatch.Stop();
                logger.Success($"Hoàn thành sau {Format(stopwatch.Elapsed)}.");
                return new TaskRunResult(TaskRunStatus.Succeeded, stopwatch.Elapsed, null, fileLogger?.FilePath);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                logger.Warning($"Đã huỷ sau {Format(stopwatch.Elapsed)}.");
                return new TaskRunResult(TaskRunStatus.Cancelled, stopwatch.Elapsed, null, fileLogger?.FilePath);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.Error(ex, "Tác vụ lỗi: " + ex.Message);
                return new TaskRunResult(TaskRunStatus.Failed, stopwatch.Elapsed, ex, fileLogger?.FilePath);
            }
            finally
            {
                fileLogger?.Dispose();
            }
        }

        private static string Format(TimeSpan t) => t.ToString(@"hh\:mm\:ss");
    }
}
