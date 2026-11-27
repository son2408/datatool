using System.Windows;
using System.Windows.Media;

namespace DataTool.UI.Controls.Helpers
{
    /// <summary>
    /// Lấy brush theo môi trường từ theme (khoá DT.Brush.Env.*), có màu dự phòng
    /// nếu ứng dụng chưa nạp theme.
    /// </summary>
    public static class EnvironmentBrushes
    {
        public static Brush Get(EnvironmentLevel level)
        {
            string key = "DT.Brush.Env." + level;
            if (Application.Current?.TryFindResource(key) is Brush brush)
                return brush;

            switch (level)
            {
                case EnvironmentLevel.Development: return Frozen(Color.FromRgb(0x2E, 0x7D, 0x32));
                case EnvironmentLevel.Test: return Frozen(Color.FromRgb(0xEF, 0x6C, 0x00));
                case EnvironmentLevel.Production: return Frozen(Color.FromRgb(0xC6, 0x28, 0x28));
                default: return Frozen(Color.FromRgb(0x54, 0x6E, 0x7A));
            }
        }

        public static Brush GetResource(string key, Color fallback)
        {
            if (Application.Current?.TryFindResource(key) is Brush brush)
                return brush;
            return Frozen(fallback);
        }

        private static Brush Frozen(Color color)
        {
            var b = new SolidColorBrush(color);
            b.Freeze();
            return b;
        }
    }
}
