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
using DataTool.Configuration;
using DataTool.Core.Provinces;
using DataTool.UI.Controls;

namespace DataTool.App.Diagnostics
{
    /// <summary>
    /// Chế độ chụp ảnh giao diện: DataTool.exe --screenshots &lt;thư mục&gt;
    /// Tạo vài file cấu hình tỉnh mẫu trong thư mục tạm (không đụng tới thiết lập thật của người dùng),
    /// mở lần lượt các màn hình, lưu PNG rồi thoát. Dùng trong CI.
    /// </summary>
    internal static class ScreenshotRunner
    {
        private const double Scale = 1.5;

        public static async Task<int> RunAsync(string outputFolder, IDialogService dialogs)
        {
            Directory.CreateDirectory(outputFolder);
            string sandbox = Path.Combine(Path.GetTempPath(), "DataToolScreens_" + Guid.NewGuid().ToString("N"));
            try
            {
                var catalog = CreateSampleCatalog(sandbox);
                var vm = new MainViewModel(catalog, dialogs, SampleTaskCatalog.GetTasks());
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

                // Đang chạy Publish code
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

                // Tác vụ xoá + hộp thoại xác nhận gõ mã tỉnh
                var delete = group.Children.First(n => n.Task != null && n.Task.IsDangerous);
                delete.IsSelected = true;
                await Settle(600);
                Save(main, outputFolder, "03-tac-vu-xoa.png");

                var province = vm.SelectedProvince;
                var dialog = ConfirmDialog.Create(
                    "Xác nhận thao tác xoá",
                    $"Tác vụ \"{delete.Title}\" sẽ thay đổi dữ liệu và không thể hoàn tác.\n\n" +
                    $"Tỉnh: {province.DisplayName} ({province.Id})\nSolr key: {province.SolrKey}",
                    DialogKind.Danger, "Chạy tác vụ", "Huỷ", province.DisplayName);
                dialog.WindowStartupLocation = WindowStartupLocation.Manual;
                dialog.Left = 100;
                dialog.Top = 100;
                dialog.Show();
                await Settle(800);
                Save(dialog, outputFolder, "04-xac-nhan-xoa.png");
                dialog.Close();

                // Cửa sổ cấu hình tỉnh
                var configVm = new ConfigurationViewModel(catalog, dialogs);
                var config = new ConfigurationWindow
                {
                    DataContext = configVm,
                    Width = 1180,
                    Height = 900,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = 0,
                    Top = 0
                };
                config.Show();
                await Settle(900);
                Save(config, outputFolder, "05-cau-hinh.png");

                // Đang sửa một giá trị (hiện dấu chưa lưu)
                configVm.Selected.SolrKey = configVm.Selected.SolrKey + "_TEST";
                await Settle(500);
                Save(config, outputFolder, "06-cau-hinh-dang-sua.png");
                configVm.Selected.Reset();
                config.Hide();

                main.Hide();
                return 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(outputFolder, "error.txt"), ex.ToString());
                return 1;
            }
            finally
            {
                try { Directory.Delete(sandbox, true); } catch { /* bỏ qua */ }
            }
        }

        private static ProvinceCatalog CreateSampleCatalog(string sandbox)
        {
            string folder = Path.Combine(sandbox, "configs");
            var store = new ProvinceStore(folder);
            var samples = new[]
            {
                ("gla_64", "GLA"), ("khh_56", "KHH"), ("hph_31", "HPH"), ("bdu_74", "BDU"),
                ("bgi_24", "BGI"), ("yba_15", "YBA"), ("vlo_86", "VLO"), ("tth_46", "TTH"),
                ("tvi_84", "TVI"), ("hgi_02", "HGI"), ("tgi_82", "TGI"), ("dno_67", "DNO")
            };
            int n = 10;
            foreach (var (id, key) in samples)
            {
                var p = store.Create(id);
                p.ServiceKey = key;
                p.SolrUri = $"http://10.0.0.{n}:8696/solr";
                p.SolrKey = key + "LIS";
                p.VdmsConnectionString = $"mongodb://admin:abc123@10.0.1.{n}:30003,10.0.1.{n + 1}:30003/{key}LIS?authSource=admin&maxIdleTimeMS=180000&maxPoolSize=1000";
                p.LogConnectionString = $"mongodb://10.0.1.60:4416,10.0.1.61:4416/{key}LIS_log?replicaSet=rs-log";
                p.VdfsApiEndpoint = "http://10.0.1.253:6787/api/v1/no_auth";
                p.VdfsRabbitUri = "amqp://abc:abc@10.0.1.45:5672/";
                p.RepoEndpoint = "http://10.0.16.39:9109/DataBase/RepositoryService/rest";
                p.RabbitUri = $"amqp://vbd:vbd@10.0.8.128/LIS.{key}";
                store.Save(p);
                n += 2;
            }

            var settingsStore = new UserSettingsStore(Path.Combine(sandbox, "settings.yml"));
            settingsStore.Save(new UserSettings { ConfigFolder = folder, ActiveProvince = "gla_64" });
            return new ProvinceCatalog(settingsStore, folder);
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
            var margin = content.Margin;
            double w = content.ActualWidth + margin.Left + margin.Right;
            double h = content.ActualHeight + margin.Top + margin.Bottom;

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(window.Background ?? Brushes.White, null, new Rect(0, 0, w, h));
                dc.DrawRectangle(new VisualBrush(content) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top },
                    null, new Rect(margin.Left, margin.Top, content.ActualWidth, content.ActualHeight));
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
