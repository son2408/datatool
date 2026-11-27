using System;
using System.Windows;
using System.Windows.Threading;
using DataTool.App.Diagnostics;
using DataTool.App.Services;
using DataTool.App.ViewModels;
using DataTool.UI.Controls;

namespace DataTool.App
{
    public partial class App : Application
    {
        private bool _screenshotMode;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += OnUnhandledException;

            // Giai đoạn dựng giao diện: dùng dữ liệu mẫu.
            // Các bước sau sẽ thay bằng ConfigStore (JSON), TaskRegistry (quét ITask) và tester thật.
            var dialogs = new DialogService();

            int index = Array.IndexOf(e.Args, "--screenshots");
            if (index >= 0)
            {
                string folder = index + 1 < e.Args.Length ? e.Args[index + 1] : "screenshots";
                _screenshotMode = true;
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var code = await ScreenshotRunner.RunAsync(folder, new SampleProfileStore(), new FakeConnectionTester(), dialogs);
                Shutdown(code);
                return;
            }

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
            if (_screenshotMode)
            {
                // Không hiện hộp thoại khi chạy tự động trong CI
                Console.Error.WriteLine(e.Exception);
                e.Handled = true;
                Shutdown(2);
                return;
            }

            ConfirmDialog.ShowMessage(null, "Đã xảy ra lỗi", e.Exception.Message, DialogKind.Error);
            e.Handled = true;
        }
    }
}
