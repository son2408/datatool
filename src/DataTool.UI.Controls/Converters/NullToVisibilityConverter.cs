using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DataTool.UI.Controls.Converters
{
    /// <summary>null → Collapsed, khác null → Visible. Đặt Invert="True" để đảo.</summary>
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasValue = value != null && !(value is string s && s.Length == 0);
            if (Invert) hasValue = !hasValue;
            return hasValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
