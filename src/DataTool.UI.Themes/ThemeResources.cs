using System;
using System.Linq;
using System.Windows;

namespace DataTool.UI.Themes
{
    /// <summary>
    /// Tiện ích nạp theme DataTool vào một ứng dụng WPF bất kỳ.
    /// </summary>
    public static class ThemeResources
    {
        public static readonly Uri ThemeUri =
            new Uri("pack://application:,,,/DataTool.UI.Themes;component/Themes/Theme.xaml", UriKind.Absolute);

        /// <summary>Gộp theme vào Application.Resources nếu chưa có.</summary>
        public static void Apply(Application application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            var merged = application.Resources.MergedDictionaries;
            if (merged.Any(d => d.Source == ThemeUri)) return;
            merged.Add(new ResourceDictionary { Source = ThemeUri });
        }
    }
}
