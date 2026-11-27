using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DataTool.App.Services;
using DataTool.App.ViewModels;
using DataTool.App.Views;
using DataTool.UI.Controls;

namespace DataTool.App.Diagnostics
{
    /// <summary>
    /// Chế độ chụp ảnh giao diện: DataTool.exe --screenshots &lt;thư mục&gt;
    /// Mở lần lượt các màn hình với dữ liệu mẫu, lưu PNG rồi thoát. Dùng trong CI để xem giao diện
    /// mà không cần mở Visual Studio.
    /// </summary>
    internal static class ScreenshotRunner
    {
        private const double Scale = 1.5;

        public static async Task<int> RunAsync(string outputFolder, IProfileStore store, IConnectionTester tester, IDialogService dialogs)
        {
            Directory.CreateDirectory(outputFolder);
            try
            {
                var vm = new MainViewModel(store, tester, dialogs, SampleTaskCatalog.GetTasks());
                var main = new MainWindow
                {
                    DataContext = vm,
                    Width = 1280,
                    Height = 800,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = 0,
                    Top = 0
                };
                Application.Current.MainWindow = main;
                main.Show();
                await Settle(1000);
                Save(main, outputFolder, "01-man-hinh-chinh.png");

                // Đang chạy tác vụ Publish code trên Dev
                var group = vm.Menu.First();
                var publish = group.Children.First(n => n.Title == "Publish code");
                publish.IsSelected = true;
                await Settle(500);
                var run = vm.CurrentTask;
                run.RunCommand.Execute(null);
                await Settle(3200);
                Save(main, outputFolder, "02-dang-chay-tac-vu.png");

                for (int i = 0; i < 40 && run.IsRunning; i++)
                    await Task.Delay(250);
                await Settle(500);
                Save(main, outputFolder, "03-hoan-thanh.png");

                // Chọn Production và tác vụ xoá
                vm.SelectedEnvironment = vm.Environments.First(e => e.IsProduction);
                var delete = group.Children.First(n => n.Task != null && n.Task.IsDangerous);
                delete.IsSelected = true;
                await Settle(800);
                Save(main, outputFolder, "04-production-tac-vu-xoa.png");

                // Hộp thoại xác nhận Production
                var env = vm.SelectedEnvironment;
                var dialog = ConfirmDialog.Create(
                    "Xác nhận thao tác xoá",
                    $"Tác vụ \"{delete.Title}\" sẽ thay đổi dữ liệu và không thể hoàn tác.\n\n" +
                    $"Môi trường: {env.Name}\nCSDL: {env.Database.Host}/{env.Database.ServiceName}",
                    DialogKind.Danger, "Chạy tác vụ", "Huỷ", env.Name);
                dialog.WindowStartupLocation = WindowStartupLocation.Manual;
                dialog.Left = 100;
                dialog.Top = 100;
                dialog.Show();
                await Settle(800);
                Save(dialog, outputFolder, "05-xac-nhan-production.png");
                dialog.Close();

                // Cửa sổ cấu hình, đã bấm "Kiểm tra kết nối" CSDL
                var configVm = new ConfigurationViewModel(store, tester, dialogs);
                var config = new ConfigurationWindow
                {
                    DataContext = configVm,
                    Width = 1040,
                    Height = 1180,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = 0,
                    Top = 0
                };
                config.Show();
                await Settle(600);
                await configVm.SelectedProfile.DatabaseTest.TestCommand.ExecuteAsync(null);
                await Settle(600);
                Save(config, outputFolder, "06-cau-hinh.png");
                config.Close();

                main.Close();
                return 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(outputFolder, "error.txt"), ex.ToString());
                return 1;
            }
        }

        private static async Task Settle(int milliseconds)
        {
            await Task.Delay(milliseconds);
            await Dispatcher.Yield(DispatcherPriority.ContextIdle);
        }

        /// <summary>Chụp vùng nội dung của cửa sổ (không gồm thanh tiêu đề Windows).</summary>
        private static void Save(Window window, string folder, string fileName)
        {
            window.UpdateLayout();
            var content = (FrameworkElement)window.Content;
            double w = content.ActualWidth, h = content.ActualHeight;

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                var rect = new Rect(0, 0, w, h);
                dc.DrawRectangle(window.Background ?? Brushes.White, null, rect);
                dc.DrawRectangle(new VisualBrush(content) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top }, null, rect);
            }

            var bitmap = new RenderTargetBitmap((int)Math.Ceiling(w * Scale), (int)Math.Ceiling(h * Scale),
                96 * Scale, 96 * Scale, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = File.Create(Path.Combine(folder, fileName)))
                encoder.Save(stream);
        }
    }
}
