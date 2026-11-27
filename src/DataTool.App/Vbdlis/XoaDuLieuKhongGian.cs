using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DataTool.App.Models;
using DataTool.Core.Logging;
using DataTool.Core.Provinces;
#if HAS_VDMS
using MongoDB.Driver.Builders;
using VDMSDataHelper;
using VDMSDataHelper.Model;
#endif

namespace DataTool.App.Vbdlis
{
    /// <summary>
    /// Xoá dữ liệu không gian theo danh sách mã xã: mỗi xã xoá toàn bộ các layer không gian theo maXa.
    /// Có VDMSDataHelper (máy dev) thì xoá thật bằng DataHandler; không có (GitHub, máy khác) thì chạy giả lập.
    /// </summary>
    public static class XoaDuLieuKhongGian
    {
        public const string CodesKey = "codes";
        public const string OutputFolderKey = "output_folder";

        public static readonly IReadOnlyList<string> LayerKhongGians = new[]
        {
            "tt_thuadat",
            "diaphancaptinh",
            "diaphancaphuyen",
            "diaphancapxa",
            "diemdiadanh",
            "diemdocao",
            "diemghichu",
            "diemtoado",
            "duongbiengioi",
            "duongchigioihanhlangantoanbaove",
            "duongchigioiquyhoach",
            "duongdiagioicaphuyen",
            "duongdiagioicaptinh",
            "duongdiagioicapxa",
            "duongsat",
            "duongthuyhe",
            "matduongbo",
            "mocbiengioidiagioi",
            "mocgioihanhlangantoanbaove",
            "mocgioiquyhoach",
            "ranhgioiduong",
            "taisanganlienvoidat",
            "timduong",
            "vungthuyhe",
            "duongmepnuoc",
            "matduonggiaothong",
            "giaothongdangduong",
            "thuadatdinhvidangdiem",
            "thuadatdinhvidangvung",
        };

        /// <summary>Đổi "123" thành "00123". Trả về null nếu không phải số.</summary>
        public static string FormatMaXa(string code)
        {
            return int.TryParse(code?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int xaId)
                ? xaId.ToString("00000", CultureInfo.InvariantCulture)
                : null;
        }

        public static async Task RunAsync(FakeTaskContext ctx)
        {
            var codes = ctx.GetCodeList(CodesKey);
            string folder = ctx.GetParameter(OutputFolderKey);
            Directory.CreateDirectory(folder);
            string resultFile = Path.Combine(folder, $"xoa_khong_gian_{ctx.Province.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            int totalSteps = codes.Count * LayerKhongGians.Count;
            int step = 0, okXa = 0, failedXa = 0;
            var result = new StringBuilder("ma_xa,layer,ket_qua,ghi_chu\r\n");

#if HAS_VDMS
            ctx.Info($"Kết nối dữ liệu tỉnh {ctx.Province.DisplayName}...");
            var dataHandler = CreateDataHandler(ctx.Province);
#else
            ctx.Warn("Bản build này không có VDMSDataHelper: CHẠY GIẢ LẬP, không xoá dữ liệu thật.");
#endif
            try
            {
                foreach (var code in codes)
                {
                    string maXa = FormatMaXa(code);
                    if (maXa == null)
                    {
                        failedXa++;
                        step += LayerKhongGians.Count;
                        ctx.Log(LogLevel.Error, $"Mã xã \"{code}\" không phải số, bỏ qua.");
                        result.Append(Csv(code)).Append(",,loi,ma xa khong hop le\r\n");
                        continue;
                    }

                    ctx.Info($"===== Xã {maXa}: xoá {LayerKhongGians.Count} layer =====");
                    bool xaOk = true;

                    foreach (var layerName in LayerKhongGians)
                    {
                        ctx.CancellationToken.ThrowIfCancellationRequested();
                        ctx.Report(step * 100.0 / totalSteps, $"Xã {maXa}: {layerName}");

                        try
                        {
#if HAS_VDMS
                            var query = Query.EQ("maXa", maXa);
                            await Task.Run(() => dataHandler.DeleteData(layerName, query), ctx.CancellationToken);
#else
                            await Task.Delay(30, ctx.CancellationToken);
#endif
                            ctx.Log(LogLevel.Debug, $"DELETE {layerName.ToUpper()}__{maXa}: xong");
                            result.Append(maXa).Append(',').Append(layerName).Append(",da_xoa,\r\n");
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                            // Lỗi một layer: ghi lại rồi xoá tiếp các layer khác
                            xaOk = false;
                            ctx.Log(LogLevel.Error, $"DELETE {layerName.ToUpper()}__{maXa}: lỗi {ex.Message}");
                            result.Append(maXa).Append(',').Append(layerName).Append(",loi,").Append(Csv(ex.Message)).Append("\r\n");
                        }

                        step++;
                    }

                    if (xaOk) { okXa++; ctx.Info($"Xã {maXa}: hoàn tất."); }
                    else { failedXa++; ctx.Warn($"Xã {maXa}: có layer bị lỗi, xem file kết quả."); }
                }

                ctx.Report(100, "Hoàn tất");
            }
            finally
            {
#if HAS_VDMS
                (dataHandler as IDisposable)?.Dispose();
#endif
                // Luôn ghi file kết quả, kể cả khi bị huỷ giữa chừng
                File.WriteAllText(resultFile, result.ToString(), new UTF8Encoding(true));
                ctx.Info($"Xã xong: {okXa}, xã có lỗi: {failedXa}. File kết quả: {resultFile}");
            }
        }

#if HAS_VDMS
        /// <summary>Tạo DataHandler từ cấu hình tỉnh (file .yml) thay cho app.config.</summary>
        private static DataHandler CreateDataHandler(ProvinceProfile p)
        {
            // Thư viện cũ còn đọc app.config: ghi đè bằng giá trị của tỉnh đang chọn trước khi khởi tạo
            AppConfigOverride.Apply(p);

            var mongo = new MongoDBConfig
            {
                VDMSConnectionString = p.VdmsConnectionString,
                LogConnectionString = p.LogConnectionString,
                GridFsConnectionString = p.Get("gridfs_connection_string"),
                CreatedUser = Environment.UserName
            };
            var solr = new SolrConfig
            {
                Uri = p.SolrUri,
                Key = p.SolrKey
                // SearchConfigurationSection: để trống. Nếu DataHandler cần khi xoá, tạo ở đây (HostServer = p.SolrUri...).
            };
            return new DataHandler(mongo, solr);
        }
#endif

        private static string Csv(string s) => "\"" + (s ?? string.Empty).Replace("\"", "\"\"") + "\"";
    }
}
