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
                Description = "Xoá dữ liệu không gian (hình thể) của các thửa đất theo danh sách mã. " +
                              "Thao tác không thể hoàn tác; kết quả được lưu ra file trong thư mục đã chọn.",
                IsDangerous = true,
                Icon = PackIconKind.MapMarkerRemoveOutline,
                Parameters = new[]
                {
                    new TaskParameter(DeleteCodesKey, "Danh sách mã", TaskParameterKind.CodeList)
                    {
                        Hint = "ngăn cách bằng dấu phẩy hoặc xuống dòng",
                        Placeholder = "12345, 12346, 12347"
                    },
                    new TaskParameter(DeleteOutputKey, "Thư mục lưu file", TaskParameterKind.Folder)
                    {
                        Hint = "file kết quả xoá theo danh sách",
                        DefaultValue = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DataTool", "xoa-thua-khong-gian")
                    }
                },
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

        public const string DeleteCodesKey = "codes";
        public const string DeleteOutputKey = "output_folder";

        private static async Task SimulateDeleteAsync(FakeTaskContext ctx)
        {
            var codes = ctx.GetCodeList(DeleteCodesKey);
            string folder = ctx.GetParameter(DeleteOutputKey);
            Directory.CreateDirectory(folder);
            string file = Path.Combine(folder, $"xoa_{ctx.Province.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            ctx.Info($"Tỉnh {ctx.Province.DisplayName}: kết nối MongoDB...");
            await Task.Delay(600, ctx.CancellationToken);
            ctx.Info($"Xoá dữ liệu không gian của {codes.Count} mã.");

            var result = new StringBuilder("ma,ket_qua,ghi_chu\r\n");
            int deleted = 0, skipped = 0;
            try
            {
                for (int i = 0; i < codes.Count; i++)
                {
                    ctx.CancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(120, ctx.CancellationToken);

                    // Giả lập: mã kết thúc bằng 0 coi như không có dữ liệu không gian
                    if (codes[i].EndsWith("0"))
                    {
                        skipped++;
                        ctx.Warn($"Mã {codes[i]}: không có dữ liệu không gian, bỏ qua.");
                        result.Append(codes[i]).Append(",bo_qua,khong co du lieu khong gian\r\n");
                    }
                    else
                    {
                        deleted++;
                        ctx.Info($"Mã {codes[i]}: đã xoá.");
                        result.Append(codes[i]).Append(",da_xoa,\r\n");
                    }

                    ctx.Report((i + 1) * 100.0 / codes.Count, $"Đang xoá {i + 1}/{codes.Count}");
                }

                ctx.Info($"Đồng bộ chỉ mục Solr ({ctx.Province.SolrKey})...");
                await Task.Delay(500, ctx.CancellationToken);
            }
            finally
            {
                // Luôn ghi file kết quả, kể cả khi bị huỷ giữa chừng
                File.WriteAllText(file, result.ToString(), new UTF8Encoding(true));
                ctx.Info($"Đã xoá {deleted}, bỏ qua {skipped}. File kết quả: {file}");
            }
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
