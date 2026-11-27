using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DataTool.UI.Controls.Helpers;
using MaterialDesignThemes.Wpf;

namespace DataTool.UI.Controls
{
    /// <summary>
    /// Hiển thị kết quả kiểm tra kết nối: đang kiểm tra (vòng xoay), thành công (dấu tích xanh),
    /// thất bại (dấu X đỏ) kèm thông điệp.
    /// </summary>
    public partial class ConnectionStatusIndicator : UserControl
    {
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(nameof(State), typeof(ConnectionTestState), typeof(ConnectionStatusIndicator),
                new PropertyMetadata(ConnectionTestState.NotTested, (d, e) => ((ConnectionStatusIndicator)d).UpdateVisual()));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(ConnectionStatusIndicator),
                new PropertyMetadata(string.Empty, (d, e) => ((ConnectionStatusIndicator)d).UpdateVisual()));

        public ConnectionStatusIndicator()
        {
            InitializeComponent();
            UpdateVisual();
        }

        public ConnectionTestState State
        {
            get => (ConnectionTestState)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        private void UpdateVisual()
        {
            MessageText.Text = Message ?? string.Empty;
            Spinner.Visibility = State == ConnectionTestState.Testing ? Visibility.Visible : Visibility.Collapsed;

            switch (State)
            {
                case ConnectionTestState.Succeeded:
                    SetIcon(PackIconKind.CheckCircle, EnvironmentBrushes.GetResource("DT.Brush.Success", Color.FromRgb(0x2E, 0x7D, 0x32)));
                    break;
                case ConnectionTestState.Failed:
                    SetIcon(PackIconKind.CloseCircle, EnvironmentBrushes.GetResource("DT.Brush.Danger", Color.FromRgb(0xC6, 0x28, 0x28)));
                    break;
                default:
                    StateIcon.Visibility = Visibility.Collapsed;
                    MessageText.Foreground = EnvironmentBrushes.GetResource("DT.Brush.TextSecondary", Color.FromRgb(0x5F, 0x6B, 0x7A));
                    break;
            }
        }

        private void SetIcon(PackIconKind kind, Brush brush)
        {
            StateIcon.Kind = kind;
            StateIcon.Foreground = brush;
            StateIcon.Visibility = Visibility.Visible;
            MessageText.Foreground = brush;
        }
    }
}
