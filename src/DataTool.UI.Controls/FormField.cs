using System.Windows;
using System.Windows.Controls;

namespace DataTool.UI.Controls
{
    /// <summary>
    /// Ô nhập có nhãn phía trên: Label (ví dụ tên khoá "solr_uri"), Hint (chú thích ngắn) và Text (giá trị, binding hai chiều).
    /// Dùng cho mọi form cấu hình thay vì viết lặp TextBlock + TextBox.
    /// </summary>
    public class FormField : Control
    {
        static FormField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FormField), new FrameworkPropertyMetadata(typeof(FormField)));
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(FormField), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register(nameof(Hint), typeof(string), typeof(FormField), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(FormField),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty UseMonospaceProperty =
            DependencyProperty.Register(nameof(UseMonospace), typeof(bool), typeof(FormField), new PropertyMetadata(true));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(FormField), new PropertyMetadata(false));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>Chữ đơn cách cho chuỗi kết nối, URL. Mặc định bật.</summary>
        public bool UseMonospace
        {
            get => (bool)GetValue(UseMonospaceProperty);
            set => SetValue(UseMonospaceProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }
    }
}
