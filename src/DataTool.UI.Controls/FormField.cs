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

        public static readonly DependencyProperty IsMultilineProperty =
            DependencyProperty.Register(nameof(IsMultiline), typeof(bool), typeof(FormField), new PropertyMetadata(false));

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(FormField), new PropertyMetadata(null));

        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register(nameof(Footer), typeof(string), typeof(FormField), new PropertyMetadata(null));

        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register(nameof(Error), typeof(string), typeof(FormField),
                new PropertyMetadata(null, (d, e) => d.SetValue(HasErrorPropertyKey, !string.IsNullOrWhiteSpace(e.NewValue as string))));

        private static readonly DependencyPropertyKey HasErrorPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(HasError), typeof(bool), typeof(FormField), new PropertyMetadata(false));

        public static readonly DependencyProperty HasErrorProperty = HasErrorPropertyKey.DependencyProperty;

        public bool HasError => (bool)GetValue(HasErrorProperty);

        /// <summary>Ô nhập nhiều dòng (cao hơn, cho xuống dòng, có thanh cuộn).</summary>
        public bool IsMultiline
        {
            get => (bool)GetValue(IsMultilineProperty);
            set => SetValue(IsMultilineProperty, value);
        }

        /// <summary>Chữ mờ gợi ý khi ô trống.</summary>
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        /// <summary>Dòng chú thích nhỏ bên dưới ô (ví dụ "Đã nhập 12 mã").</summary>
        public string Footer
        {
            get => (string)GetValue(FooterProperty);
            set => SetValue(FooterProperty, value);
        }

        /// <summary>Thông báo lỗi hiển thị màu đỏ bên dưới ô (ưu tiên hơn Footer).</summary>
        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

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
