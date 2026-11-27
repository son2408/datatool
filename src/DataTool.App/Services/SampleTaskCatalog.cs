using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DataTool.App.Models;
using DataTool.Core.Tasks;
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
                Description = "Xoá dữ liệu không gian của các xã theo danh sách mã xã (29 layer: thửa đất, địa phận, giao thông, thuỷ hệ...). " +
                              "Thao tác không thể hoàn tác; kết quả được lưu ra file trong thư mục đã chọn.",
                IsDangerous = true,
                Icon = PackIconKind.MapMarkerRemoveOutline,
                Parameters = new[]
                {
                    new TaskParameter(DeleteCodesKey, "Danh sách mã xã", TaskParameterKind.CodeList)
                    {
                        Hint = "ngăn cách bằng dấu phẩy hoặc xuống dòng; mỗi xã xoá 29 layer không gian theo maXa",
                        Placeholder = "12345, 12346, 12347"
                    },
                    new TaskParameter(DeleteOutputKey, "Thư mục lưu file", TaskParameterKind.Folder)
                    {
                        Hint = "file kết quả xoá theo danh sách",
                        DefaultValue = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DataTool", "xoa-thua-khong-gian")
                    }
                },
                Execute = Vbdlis.XoaDuLieuKhongGian.RunAsync
            },
            new TaskDescriptor
            {
                MenuPath = "VBDLIS/Publish code",
                Description = "Đóng gói và triển khai phiên bản mã nguồn mới lên môi trường đã chọn.",
                Icon = PackIconKind.RocketLaunchOutline,
                Execute = SimulatePublishAsync
            }
        };

        public const string DeleteCodesKey = Vbdlis.XoaDuLieuKhongGian.CodesKey;
        public const string DeleteOutputKey = Vbdlis.XoaDuLieuKhongGian.OutputFolderKey;

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
