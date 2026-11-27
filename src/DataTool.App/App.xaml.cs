using System.Windows;
using System.Windows.Threading;
using DataTool.App.Services;
using DataTool.App.ViewModels;
using DataTool.UI.Controls;

namespace DataTool.App
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += OnUnhandledException;

            // Giai đoạn dựng giao diện: dùng dữ liệu mẫu.
            // Các bước sau sẽ thay bằng ConfigStore (JSON), TaskRegistry (quét ITask) và tester thật.
            var dialogs = new DialogService();
            var mainViewModel = new MainViewModel(
                new SampleProfileStore(),
                new FakeConnectionTester(),
                dialogs,
                SampleTaskCatalog.GetTasks());

            var window = new MainWindow { DataContext = mainViewModel };
            MainWindow = window;
            window.Show();
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ConfirmDialog.ShowMessage(null, "Đã xảy ra lỗi", e.Exception.Message, DialogKind.Error);
            e.Handled = true;
        }
    }
}
