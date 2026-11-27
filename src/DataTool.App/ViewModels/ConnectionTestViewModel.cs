using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTool.App.Services;
using DataTool.UI.Controls;

namespace DataTool.App.ViewModels
{
    /// <summary>Trạng thái và lệnh cho một nút "Kiểm tra kết nối".</summary>
    public class ConnectionTestViewModel : ObservableObject
    {
        private readonly Func<CancellationToken, Task<ConnectionTestOutcome>> _test;
        private ConnectionTestState _state = ConnectionTestState.NotTested;
        private string _message = "Chưa kiểm tra";

        public ConnectionTestViewModel(Func<CancellationToken, Task<ConnectionTestOutcome>> test)
        {
            _test = test;
            TestCommand = new AsyncRelayCommand(RunAsync);
        }

        public ConnectionTestState State
        {
            get => _state;
            private set => SetProperty(ref _state, value);
        }

        public string Message
        {
            get => _message;
            private set => SetProperty(ref _message, value);
        }

        public AsyncRelayCommand TestCommand { get; }

        /// <summary>Gọi khi người dùng sửa thông số để xoá kết quả cũ.</summary>
        public void Reset()
        {
            if (State == ConnectionTestState.Testing) return;
            State = ConnectionTestState.NotTested;
            Message = "Chưa kiểm tra";
        }

        private async Task RunAsync()
        {
            State = ConnectionTestState.Testing;
            Message = "Đang kiểm tra...";
            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                {
                    var outcome = await _test(cts.Token);
                    State = outcome.Success ? ConnectionTestState.Succeeded : ConnectionTestState.Failed;
                    Message = outcome.Message;
                }
            }
            catch (OperationCanceledException)
            {
                State = ConnectionTestState.Failed;
                Message = "Hết thời gian chờ (15 giây).";
            }
            catch (Exception ex)
            {
                State = ConnectionTestState.Failed;
                Message = ex.Message;
            }
        }
    }
}
