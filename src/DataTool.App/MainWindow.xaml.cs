using System.Windows;
using DataTool.App.ViewModels;

namespace DataTool.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // TreeView.SelectedItem không binding được, nên chuyển sang ViewModel ở đây.
        private void OnMenuSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectedNode = e.NewValue as MenuNodeViewModel;
        }
    }
}
