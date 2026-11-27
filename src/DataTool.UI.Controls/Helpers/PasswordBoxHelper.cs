using System.Windows;
using System.Windows.Controls;

namespace DataTool.UI.Controls.Helpers
{
    /// <summary>
    /// Cho phép binding hai chiều với PasswordBox.Password (WPF mặc định không hỗ trợ).
    /// Cách dùng: helpers:PasswordBoxHelper.BoundPassword="{Binding Password}"
    /// </summary>
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false));

        public static string GetBoundPassword(DependencyObject d) => (string)d.GetValue(BoundPasswordProperty);
        public static void SetBoundPassword(DependencyObject d, string value) => d.SetValue(BoundPasswordProperty, value);

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is PasswordBox box)) return;

            box.PasswordChanged -= OnPasswordChanged;
            if (!(bool)box.GetValue(IsUpdatingProperty))
                box.Password = (string)e.NewValue ?? string.Empty;
            box.PasswordChanged += OnPasswordChanged;
        }

        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var box = (PasswordBox)sender;
            box.SetValue(IsUpdatingProperty, true);
            SetBoundPassword(box, box.Password);
            box.SetValue(IsUpdatingProperty, false);
        }
    }
}
