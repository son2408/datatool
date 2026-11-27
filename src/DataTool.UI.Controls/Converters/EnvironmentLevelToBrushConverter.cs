using System;
using System.Globalization;
using System.Windows.Data;
using DataTool.UI.Controls.Helpers;

namespace DataTool.UI.Controls.Converters
{
    /// <summary>Đổi EnvironmentLevel thành brush màu của môi trường.</summary>
    public sealed class EnvironmentLevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = value is EnvironmentLevel l ? l : EnvironmentLevel.Other;
            return EnvironmentBrushes.Get(level);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
