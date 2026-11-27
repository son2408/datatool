using System;
using System.Globalization;
using System.Windows.Data;

namespace DataTool.UI.Controls.Converters
{
    /// <summary>Đảo giá trị bool: true → false, false → true.</summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => !(value is bool b && b);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => !(value is bool b && b);
    }
}
