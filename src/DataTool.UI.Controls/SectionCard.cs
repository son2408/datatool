using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace DataTool.UI.Controls
{
    /// <summary>
    /// Thẻ nhóm nội dung: biểu tượng + tiêu đề (Header) + nội dung (Content).
    /// HeaderActions đặt thêm nút ở góc phải tiêu đề nếu cần.
    /// </summary>
    public class SectionCard : HeaderedContentControl
    {
        static SectionCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SectionCard), new FrameworkPropertyMetadata(typeof(SectionCard)));
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(PackIconKind), typeof(SectionCard), new PropertyMetadata(PackIconKind.CardOutline));

        public static readonly DependencyProperty HeaderActionsProperty =
            DependencyProperty.Register(nameof(HeaderActions), typeof(object), typeof(SectionCard), new PropertyMetadata(null));

        public PackIconKind Icon
        {
            get => (PackIconKind)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public object HeaderActions
        {
            get => GetValue(HeaderActionsProperty);
            set => SetValue(HeaderActionsProperty, value);
        }
    }
}
