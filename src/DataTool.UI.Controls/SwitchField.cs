using System.Windows;
using System.Windows.Controls;

namespace DataTool.UI.Controls
{
    /// <summary>
    /// Công tắc bật/tắt nhỏ gọn có nhãn, hiển thị giá trị true/false bên cạnh.
    /// </summary>
    public class SwitchField : Control
    {
        static SwitchField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SwitchField), new FrameworkPropertyMetadata(typeof(SwitchField)));
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(SwitchField), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(SwitchField),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty OnTextProperty =
            DependencyProperty.Register(nameof(OnText), typeof(string), typeof(SwitchField), new PropertyMetadata("true"));

        public static readonly DependencyProperty OffTextProperty =
            DependencyProperty.Register(nameof(OffText), typeof(string), typeof(SwitchField), new PropertyMetadata("false"));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public string OnText
        {
            get => (string)GetValue(OnTextProperty);
            set => SetValue(OnTextProperty, value);
        }

        public string OffText
        {
            get => (string)GetValue(OffTextProperty);
            set => SetValue(OffTextProperty, value);
        }
    }
}
