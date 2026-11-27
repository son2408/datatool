using System;
using System.IO;

namespace DataTool.App.Services
{
    /// <summary>Thư mục dữ liệu của ứng dụng: %AppData%\DataTool.</summary>
    public static class AppPaths
    {
        public static string DataFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DataTool");

        public static string ConfigFile => Path.Combine(DataFolder, "config.json");

        public static string LogFolder => Path.Combine(DataFolder, "logs");
    }
}
