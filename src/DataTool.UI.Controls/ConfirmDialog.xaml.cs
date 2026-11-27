using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DataTool.UI.Controls.Helpers;
using MaterialDesignThemes.Wpf;

namespace DataTool.UI.Controls
{
    /// <summary>
    /// Hộp thoại xác nhận/thông báo dùng chung.
    /// Khi truyền requiredText, người dùng phải gõ đúng chuỗi đó thì nút xác nhận mới bật
    /// (dùng cho thao tác nguy hiểm trên Production).
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        private string _requiredText;

        public ConfirmDialog()
        {
            InitializeComponent();
        }

        /// <summary>Hiện hộp thoại xác nhận. Trả về true nếu người dùng đồng ý.</summary>
        /// <param name="owner">Cửa sổ cha, null thì lấy cửa sổ đang active.</param>
        /// <param name="cancelText">null để ẩn nút huỷ (hộp thoại thông báo).</param>
        /// <param name="requiredText">Chuỗi người dùng phải gõ lại để xác nhận, null nếu không cần.</param>
        public static bool Show(Window owner, string title, string message,
            DialogKind kind = DialogKind.Question,
            string confirmText = "Đồng ý",
            string cancelText = "Huỷ",
            string requiredText = null)
        {
            var dialog = Create(title, message, kind, confirmText, cancelText, requiredText);

            var resolvedOwner = owner ?? FindActiveWindow();
            if (resolvedOwner != null && resolvedOwner != dialog && resolvedOwner.IsVisible)
                dialog.Owner = resolvedOwner;
            else
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            return dialog.ShowDialog() == true;
        }

        /// <summary>Tạo hộp thoại đã cấu hình nhưng chưa hiển thị (dùng khi cần Show không chặn, ví dụ chụp ảnh).</summary>
        public static ConfirmDialog Create(string title, string message,
            DialogKind kind = DialogKind.Question,
            string confirmText = "Đồng ý",
            string cancelText = "Huỷ",
            string requiredText = null)
        {
            var dialog = new ConfirmDialog();
            dialog.Configure(title, message, kind, confirmText, cancelText, requiredText);
            return dialog;
        }

        /// <summary>Hộp thoại chỉ có nút Đóng.</summary>
        public static void ShowMessage(Window owner, string title, string message, DialogKind kind = DialogKind.Information)
            => Show(owner, title, message, kind, "Đóng", null);

        private void Configure(string title, string message, DialogKind kind, string confirmText, string cancelText, string requiredText)
        {
            Title = string.IsNullOrEmpty(title) ? "DataTool" : title;
            TitleText.Text = title;
            MessageText.Text = message;
            ConfirmButton.Content = confirmText ?? "Đồng ý";

            if (cancelText == null)
                CancelButton.Visibility = Visibility.Collapsed;
            else
                CancelButton.Content = cancelText;

            ApplyKind(kind);

            _requiredText = string.IsNullOrWhiteSpace(requiredText) ? null : requiredText;
            if (_requiredText != null)
            {
                RequiredPanel.Visibility = Visibility.Visible;
                RequiredRun.Text = _requiredText;
                ConfirmButton.IsEnabled = false;
                ConfirmButton.IsDefault = false;
                Loaded += (s, e) => InputBox.Focus();
            }
            else
            {
                Loaded += (s, e) => (CancelButton.Visibility == Visibility.Visible ? CancelButton : ConfirmButton).Focus();
            }
        }

        private void ApplyKind(DialogKind kind)
        {
            switch (kind)
            {
                case DialogKind.Information:
                    SetHeader(PackIconKind.InformationOutline, "DT.Brush.Info", Color.FromRgb(0x15, 0x65, 0xC0));
                    break;
                case DialogKind.Warning:
                    SetHeader(PackIconKind.AlertOutline, "DT.Brush.Warning", Color.FromRgb(0xEF, 0x6C, 0x00));
                    break;
                case DialogKind.Danger:
                    SetHeader(PackIconKind.AlertOctagonOutline, "DT.Brush.Danger", Color.FromRgb(0xC6, 0x28, 0x28));
                    if (TryFindResource("DT.Button.Danger") is Style danger)
                        ConfirmButton.Style = danger;
                    break;
                case DialogKind.Error:
                    SetHeader(PackIconKind.CloseCircleOutline, "DT.Brush.Danger", Color.FromRgb(0xC6, 0x28, 0x28));
                    break;
                default:
                    SetHeader(PackIconKind.HelpCircleOutline, "DT.Brush.Primary", Color.FromRgb(0x3F, 0x51, 0xB5));
                    break;
            }
        }

        private void SetHeader(PackIconKind icon, string brushKey, Color fallback)
        {
            HeaderIcon.Kind = icon;
            HeaderIcon.Foreground = EnvironmentBrushes.GetResource(brushKey, fallback);
        }

        private void OnInputChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_requiredText == null) return;
            bool matches = string.Equals(InputBox.Text?.Trim(), _requiredText, StringComparison.Ordinal);
            ConfirmButton.IsEnabled = matches;
            ConfirmButton.IsDefault = matches;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private static Window FindActiveWindow()
        {
            var app = Application.Current;
            if (app == null) return null;
            return app.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? app.MainWindow;
        }
    }
}
