using System;
using System.IO;

namespace DataTool.App.Services
{
    /// <summary>Thư mục dữ liệu của ứng dụng: %AppData%\DataTool.</summary>
    public static class AppPaths
    {
        public static string DataFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DataTool");

        /// <summary>Thiết lập người dùng: thư mục cấu hình, tỉnh đang dùng.</summary>
        public static string SettingsFile => Path.Combine(DataFolder, "settings.yml");

        /// <summary>Thư mục cấu hình tỉnh mặc định nếu người dùng chưa chọn.</summary>
        public static string DefaultConfigFolder => Path.Combine(DataFolder, "configs");

        public static string LogFolder => Path.Combine(DataFolder, "logs");
    }
}
