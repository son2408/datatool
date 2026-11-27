using System.Collections.Generic;
using System.Threading.Tasks;
using DataTool.App.Models;
using MaterialDesignThemes.Wpf;

namespace DataTool.App.Services
{
    /// <summary>
    /// Danh sách tác vụ mẫu cho giai đoạn dựng giao diện.
    /// Ở bước Core + VBDLIS, danh sách này sẽ được sinh tự động từ các class implement ITask.
    /// </summary>
    public static class SampleTaskCatalog
    {
        public static IReadOnlyList<TaskDescriptor> GetTasks() => new[]
        {
            new TaskDescriptor
            {
                MenuPath = "VBDLIS/Xoá dữ liệu thửa đất không gian",
                Description = "Xoá dữ liệu không gian (hình thể) của các thửa đất theo điều kiện lọc. " +
                              "Thao tác không thể hoàn tác, hãy sao lưu trước khi chạy.",
                IsDangerous = true,
                Icon = PackIconKind.MapMarkerRemoveOutline,
                Execute = SimulateDeleteAsync
            },
            new TaskDescriptor
            {
                MenuPath = "VBDLIS/Publish code",
                Description = "Đóng gói và triển khai phiên bản mã nguồn mới lên môi trường đã chọn.",
                Icon = PackIconKind.RocketLaunchOutline,
                Execute = SimulatePublishAsync
            }
        };

        private static async Task SimulateDeleteAsync(FakeTaskContext ctx)
        {
            const int total = 250;
            ctx.Info($"Kết nối CSDL {ctx.Environment.Database.Host}/{ctx.Environment.Database.ServiceName}...");
            await Task.Delay(600, ctx.CancellationToken);
            ctx.Info($"Tìm thấy {total} thửa đất cần xoá dữ liệu không gian.");

            for (int i = 1; i <= total; i++)
            {
                ctx.CancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(25, ctx.CancellationToken);

                if (i % 25 == 0)
                    ctx.Info($"Đã xoá {i}/{total} thửa.");
                if (i == 137)
                    ctx.Warn("Thửa 137 không có dữ liệu không gian, bỏ qua.");

                ctx.Report(i * 100.0 / total, $"Đang xoá thửa {i}/{total}");
            }

            ctx.Info("Đồng bộ chỉ mục SOLR...");
            await Task.Delay(500, ctx.CancellationToken);
        }

        private static async Task SimulatePublishAsync(FakeTaskContext ctx)
        {
            string[] steps =
            {
                "Lấy mã nguồn mới nhất",
                "Build solution",
                "Chạy kiểm thử",
                "Đóng gói",
                "Sao chép lên máy chủ",
                "Khởi động lại dịch vụ"
            };

            for (int i = 0; i < steps.Length; i++)
            {
                ctx.CancellationToken.ThrowIfCancellationRequested();
                ctx.Report(i * 100.0 / steps.Length, steps[i] + "...");
                ctx.Info(steps[i] + "...");
                await Task.Delay(900, ctx.CancellationToken);
            }

            ctx.Report(100, "Hoàn tất");
        }
    }
}
