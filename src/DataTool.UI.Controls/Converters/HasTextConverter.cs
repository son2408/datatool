using System;
using System.Globalization;
using System.Windows.Data;

namespace DataTool.UI.Controls.Converters
{
    /// <summary>true nếu giá trị là chuỗi khác rỗng.</summary>
    public class HasTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => !string.IsNullOrWhiteSpace(value as string);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
