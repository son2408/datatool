using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DataTool.UI.Controls
{
    /// <summary>Hộp thoại nhập một dòng chữ, có kiểm tra hợp lệ trước khi đóng.</summary>
    public partial class InputDialog : Window
    {
        private Func<string, string> _validate;

        public InputDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => { InputBox.Focus(); InputBox.SelectAll(); };
        }

        public string Value => InputBox.Text?.Trim();

        /// <summary>Trả về chuỗi đã nhập, hoặc null nếu người dùng huỷ.</summary>
        /// <param name="validate">Trả về thông báo lỗi, hoặc null nếu hợp lệ.</param>
        public static string Show(Window owner, string title, string message, string defaultValue = "",
            Func<string, string> validate = null, string okText = "Đồng ý")
        {
            var dialog = Create(title, message, defaultValue, validate, okText);
            var resolvedOwner = owner ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (resolvedOwner != null && resolvedOwner.IsVisible)
                dialog.Owner = resolvedOwner;
            else
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            return dialog.ShowDialog() == true ? dialog.Value : null;
        }

        /// <summary>Tạo hộp thoại chưa hiển thị.</summary>
        public static InputDialog Create(string title, string message, string defaultValue = "",
            Func<string, string> validate = null, string okText = "Đồng ý")
        {
            var dialog = new InputDialog { _validate = validate };
            dialog.TitleText.Text = title;
            dialog.MessageText.Text = message;
            dialog.MessageText.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
            dialog.InputBox.Text = defaultValue ?? string.Empty;
            dialog.OkButton.Content = okText;
            return dialog;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e) => ErrorText.Visibility = Visibility.Collapsed;

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            string error = _validate?.Invoke(Value);
            if (!string.IsNullOrEmpty(error))
            {
                ErrorText.Text = error;
                ErrorText.Visibility = Visibility.Visible;
                return;
            }
            DialogResult = true;
        }
    }
}
