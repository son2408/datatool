using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTool.App.Models;
using DataTool.App.Services;
using DataTool.Core.Logging;
using DataTool.Core.Provinces;
using DataTool.Core.Tasks;

namespace DataTool.App.ViewModels
{
    /// <summary>Màn hình chạy một tác vụ: xác nhận, chạy nền, tiến trình, log, huỷ.</summary>
    public class TaskRunViewModel : ObservableObject
    {
        private readonly TaskDescriptor _task;
        private readonly Func<ProvinceProfile> _getProvince;
        private readonly IDialogService _dialogs;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private CancellationTokenSource _cts;

        private double _progress;
        private bool _isIndeterminate;
        private string _statusText = "Sẵn sàng";
        private string _elapsedText = string.Empty;
        private bool _isRunning;
        private bool _isCancelling;

        public TaskRunViewModel(TaskDescriptor task, Func<ProvinceProfile> getProvince, IDialogService dialogs)
        {
            _task = task;
            _getProvince = getProvince;
            _dialogs = dialogs;

            Parameters = new ObservableCollection<TaskParameterViewModel>(
                (task.Parameters ?? Array.Empty<TaskParameter>()).Select(p => new TaskParameterViewModel(p, dialogs)));

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

        /// <summary>Các ô nhập tham số (form tự sinh theo khai báo của tác vụ).</summary>
        public ObservableCollection<TaskParameterViewModel> Parameters { get; }
        public bool HasParameters => Parameters.Count > 0;

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
                OnPropertyChanged(nameof(IsIdle));
                RunCommand.NotifyCanExecuteChanged();
                CancelCommand.NotifyCanExecuteChanged();
                RunningChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Không chạy: cho phép sửa tham số.</summary>
        public bool IsIdle => !IsRunning;

        public AsyncRelayCommand RunCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand OpenLogFolderCommand { get; }

        private async Task RunAsync()
        {
            var province = _getProvince();
            if (province == null)
            {
                _dialogs.ShowError("Chưa chọn tỉnh", "Hãy chọn tỉnh ở góc trên bên phải (hoặc trong Cấu hình) trước khi chạy.");
                return;
            }

            // Kiểm tra tất cả tham số, hiện lỗi dưới từng ô
            bool valid = true;
            foreach (var p in Parameters)
                valid &= p.Validate();
            if (!valid)
            {
                StatusText = "Thiếu thông tin, hãy kiểm tra các ô báo đỏ.";
                return;
            }

            if (!ConfirmRun(province))
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

            AddLog(LogLevel.Info, $"Bắt đầu \"{Title}\" trên tỉnh {province.DisplayName}.");
            var values = Parameters.ToDictionary(p => p.Key, p => p.Value ?? string.Empty);

            try
            {
                var context = new FakeTaskContext(province, progress, log, _cts.Token, values);
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

        private bool ConfirmRun(ProvinceProfile province)
        {
            if (!IsDangerous) return true;

            // Tác vụ xoá luôn phải gõ lại mã tỉnh
            return _dialogs.ConfirmDanger("Xác nhận thao tác xoá", BuildConfirmMessage(province), province.DisplayName, "Chạy tác vụ");
        }

        /// <summary>Nội dung hộp xác nhận: tác vụ, tỉnh và tóm tắt tham số.</summary>
        public string BuildConfirmMessage(ProvinceProfile province)
        {
            return
                $"Tác vụ \"{Title}\" sẽ thay đổi dữ liệu và không thể hoàn tác.\n\n" +
                $"Tỉnh: {province.DisplayName} ({province.Id})\n" +
                $"Solr key: {province.SolrKey}" +
                DescribeParameters();
        }

        /// <summary>Tóm tắt tham số để người dùng kiểm tra lại trong hộp xác nhận.</summary>
        private string DescribeParameters()
        {
            if (!HasParameters) return string.Empty;
            var lines = new List<string>();
            foreach (var p in Parameters)
            {
                if (p.Kind == TaskParameterKind.CodeList)
                {
                    var codes = CodeListParser.Parse(p.Value);
                    string preview = string.Join(", ", codes.Take(10)) + (codes.Count > 10 ? $", ... (+{codes.Count - 10})" : string.Empty);
                    lines.Add($"{p.Label}: {codes.Count} mã\n   {preview}");
                }
                else if (!string.IsNullOrWhiteSpace(p.Value))
                {
                    lines.Add($"{p.Label}: {p.Value}");
                }
            }
            return lines.Count == 0 ? string.Empty : "\n\n" + string.Join("\n", lines);
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
