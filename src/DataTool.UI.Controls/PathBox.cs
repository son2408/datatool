using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DataTool.UI.Controls
{
    /// <summary>
    /// Ô hiển thị đường dẫn (thư mục hoặc file) kèm nút "Chọn..." gắn liền bên phải.
    /// Bấm vào ô hoặc nút đều chạy BrowseCommand.
    /// </summary>
    public class PathBox : Control
    {
        static PathBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PathBox), new FrameworkPropertyMetadata(typeof(PathBox)));
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register(nameof(Path), typeof(string), typeof(PathBox), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(PathBox), new PropertyMetadata("Chưa chọn thư mục"));

        public static readonly DependencyProperty BrowseTextProperty =
            DependencyProperty.Register(nameof(BrowseText), typeof(string), typeof(PathBox), new PropertyMetadata("Chọn..."));

        public static readonly DependencyProperty BrowseCommandProperty =
            DependencyProperty.Register(nameof(BrowseCommand), typeof(ICommand), typeof(PathBox), new PropertyMetadata(null));

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public string BrowseText
        {
            get => (string)GetValue(BrowseTextProperty);
            set => SetValue(BrowseTextProperty, value);
        }

        public ICommand BrowseCommand
        {
            get => (ICommand)GetValue(BrowseCommandProperty);
            set => SetValue(BrowseCommandProperty, value);
        }
    }
}
