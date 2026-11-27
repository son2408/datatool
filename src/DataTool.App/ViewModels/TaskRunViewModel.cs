using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTool.App.Models;
using DataTool.App.Services;

namespace DataTool.App.ViewModels
{
    /// <summary>Màn hình chạy một tác vụ: xác nhận, chạy nền, tiến trình, log, huỷ.</summary>
    public class TaskRunViewModel : ObservableObject
    {
        private readonly TaskDescriptor _task;
        private readonly Func<EnvironmentProfile> _getEnvironment;
        private readonly IDialogService _dialogs;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private CancellationTokenSource _cts;

        private double _progress;
        private bool _isIndeterminate;
        private string _statusText = "Sẵn sàng";
        private string _elapsedText = string.Empty;
        private bool _isRunning;
        private bool _isCancelling;

        public TaskRunViewModel(TaskDescriptor task, Func<EnvironmentProfile> getEnvironment, IDialogService dialogs)
        {
            _task = task;
            _getEnvironment = getEnvironment;
            _dialogs = dialogs;

            RunCommand = new AsyncRelayCommand(RunAsync, () => !IsRunning);
            CancelCommand = new RelayCommand(Cancel, () => IsRunning && !_isCancelling);
            OpenLogFolderCommand = new RelayCommand(() => _dialogs.OpenFolder(AppPaths.LogFolder));
        }

        /// <summary>Báo cho MainViewModel biết khi trạng thái chạy thay đổi.</summary>
        public event EventHandler RunningChanged;

        public string Title => _task.Title;
        public string Group => _task.Group;
        public string Description => _task.Description;
        public bool IsDangerous => _task.IsDangerous;

        public ObservableCollection<LogEntry> Logs { get; } = new ObservableCollection<LogEntry>();

        public double Progress
        {
            get => _progress;
            private set => SetProperty(ref _progress, value);
        }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            private set => SetProperty(ref _isIndeterminate, value);
        }

        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        public string ElapsedText
        {
            get => _elapsedText;
            private set => SetProperty(ref _elapsedText, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (!SetProperty(ref _isRunning, value)) return;
                RunCommand.NotifyCanExecuteChanged();
                CancelCommand.NotifyCanExecuteChanged();
                RunningChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public AsyncRelayCommand RunCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand OpenLogFolderCommand { get; }

        private async Task RunAsync()
        {
            var env = _getEnvironment();
            if (env == null)
            {
                _dialogs.ShowError("Chưa chọn môi trường", "Hãy chọn một bộ cấu hình ở góc trên bên phải trước khi chạy.");
                return;
            }

            if (!ConfirmRun(env))
            {
                AddLog(LogLevel.Warning, "Người dùng không xác nhận, tác vụ chưa chạy.");
                return;
            }

            Logs.Clear();
            _cts = new CancellationTokenSource();
            _isCancelling = false;
            IsRunning = true;
            IsIndeterminate = true;
            Progress = 0;
            StatusText = "Đang khởi động...";
            _stopwatch.Restart();

            var progress = new Progress<ProgressInfo>(p =>
            {
                IsIndeterminate = false;
                Progress = Math.Max(0, Math.Min(100, p.Percent));
                if (!string.IsNullOrEmpty(p.Message)) StatusText = p.Message;
                UpdateElapsed();
            });
            var log = new Progress<LogEntry>(entry => { Logs.Add(entry); UpdateElapsed(); });

            AddLog(LogLevel.Info, $"Bắt đầu \"{Title}\" trên môi trường {env.Name}.");

            try
            {
                var context = new FakeTaskContext(env, progress, log, _cts.Token);
                await Task.Run(() => _task.Execute(context), _cts.Token);

                Progress = 100;
                StatusText = "Hoàn thành";
                AddLog(LogLevel.Success, $"Hoàn thành sau {_stopwatch.Elapsed:hh\\:mm\\:ss}.");
            }
            catch (OperationCanceledException)
            {
                StatusText = "Đã huỷ";
                AddLog(LogLevel.Warning, "Tác vụ đã bị huỷ theo yêu cầu.");
            }
            catch (Exception ex)
            {
                StatusText = "Lỗi: " + ex.Message;
                AddLog(LogLevel.Error, ex.ToString());
            }
            finally
            {
                _stopwatch.Stop();
                UpdateElapsed();
                IsIndeterminate = false;
                _cts.Dispose();
                _cts = null;
                IsRunning = false;
            }
        }

        private bool ConfirmRun(EnvironmentProfile env)
        {
            if (!IsDangerous) return true;

            string message =
                $"Tác vụ \"{Title}\" sẽ thay đổi dữ liệu và không thể hoàn tác.\n\n" +
                $"Môi trường: {env.Name}\n" +
                $"CSDL: {env.Database.Host}/{env.Database.ServiceName}";

            // Production phải gõ lại tên môi trường
            string required = env.IsProduction ? env.Name : null;
            return _dialogs.ConfirmDanger("Xác nhận thao tác xoá", message, required, "Chạy tác vụ");
        }

        private void Cancel()
        {
            if (_cts == null) return;
            _isCancelling = true;
            CancelCommand.NotifyCanExecuteChanged();
            StatusText = "Đang huỷ...";
            _cts.Cancel();
        }

        private void AddLog(LogLevel level, string message) => Logs.Add(new LogEntry(level, message));

        private void UpdateElapsed() => ElapsedText = "Thời gian chạy: " + _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
    }
}
