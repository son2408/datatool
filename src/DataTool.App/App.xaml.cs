using System;
using System.Windows;
using System.Windows.Threading;
using DataTool.App.Diagnostics;
using DataTool.App.Services;
using DataTool.App.ViewModels;
using DataTool.Configuration;
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

            var dialogs = new DialogService();

            int index = Array.IndexOf(e.Args, "--screenshots");
            if (index >= 0)
            {
                string folder = index + 1 < e.Args.Length ? e.Args[index + 1] : "screenshots";
                _screenshotMode = true;
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var code = await ScreenshotRunner.RunAsync(folder, dialogs);
                Shutdown(code);
                return;
            }

            // Cấu hình tỉnh: đọc các file .yml trong thư mục người dùng chọn (ghi nhớ trong settings.yml).
            // Tác vụ: vẫn là dữ liệu mẫu, bước VBDLIS sẽ thay bằng TaskRegistry (quét ITask).
            var catalog = new ProvinceCatalog(new UserSettingsStore(AppPaths.SettingsFile), AppPaths.DefaultConfigFolder);
            var mainViewModel = new MainViewModel(catalog, dialogs, SampleTaskCatalog.GetTasks());

            var window = new MainWindow { DataContext = mainViewModel };
            MainWindow = window;
            window.Show();

            if (catalog.LastErrors.Count > 0)
                dialogs.ShowError("Một số file cấu hình không đọc được", string.Join(Environment.NewLine, catalog.LastErrors));
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
