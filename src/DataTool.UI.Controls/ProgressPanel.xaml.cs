using System.Windows;
using System.Windows.Controls;

namespace DataTool.UI.Controls
{
    /// <summary>
    /// Khối hiển thị tiến trình: dòng trạng thái, phần trăm, thanh tiến trình và thời gian chạy.
    /// </summary>
    public partial class ProgressPanel : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(ProgressPanel), new PropertyMetadata(0d));

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(ProgressPanel),
                new PropertyMetadata(false, (d, e) => ((ProgressPanel)d).UpdatePercentVisibility()));

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(ProgressPanel), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ElapsedTextProperty =
            DependencyProperty.Register(nameof(ElapsedText), typeof(string), typeof(ProgressPanel), new PropertyMetadata(string.Empty));

        public ProgressPanel()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        /// <summary>Giá trị 0 đến 100.</summary>
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public string ElapsedText
        {
            get => (string)GetValue(ElapsedTextProperty);
            set => SetValue(ElapsedTextProperty, value);
        }

        private void UpdatePercentVisibility()
            => PercentText.Visibility = IsIndeterminate ? Visibility.Collapsed : Visibility.Visible;
    }
}
