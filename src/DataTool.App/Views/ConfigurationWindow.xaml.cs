using System.ComponentModel;
using System.Windows;
using DataTool.App.ViewModels;

namespace DataTool.App.Views
{
    public partial class ConfigurationWindow : Window
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
        }

        // Hỏi lại nếu còn thay đổi chưa lưu
        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (DataContext is ConfigurationViewModel vm && !vm.CanClose())
                e.Cancel = true;
        }
    }
}
