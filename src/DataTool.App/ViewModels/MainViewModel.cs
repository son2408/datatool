using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTool.App.Models;
using DataTool.App.Services;
using DataTool.Core.Provinces;

namespace DataTool.App.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ProvinceCatalog _catalog;
        private readonly IDialogService _dialogs;
        private readonly List<TaskRunViewModel> _runViewModels = new List<TaskRunViewModel>();

        private ProvinceProfile _selectedProvince;
        private MenuNodeViewModel _selectedNode;
        private string _menuFilter = string.Empty;
        private bool _isBusy;
        private bool _reloading;

        public MainViewModel(ProvinceCatalog catalog, IDialogService dialogs, IEnumerable<TaskDescriptor> tasks)
        {
            _catalog = catalog;
            _dialogs = dialogs;

            foreach (var node in MenuNodeViewModel.BuildTree(tasks, CreateRunViewModel))
                Menu.Add(node);

            OpenConfigurationCommand = new RelayCommand(OpenConfiguration, () => !IsBusy);
            ReloadProvinces();
        }

        public ObservableCollection<ProvinceProfile> Provinces { get; } = new ObservableCollection<ProvinceProfile>();
        public ObservableCollection<MenuNodeViewModel> Menu { get; } = new ObservableCollection<MenuNodeViewModel>();

        /// <summary>Tỉnh đang dùng (chỉ một). Đổi ở đây cũng lưu vào thiết lập.</summary>
        public ProvinceProfile SelectedProvince
        {
            get => _selectedProvince;
            set
            {
                if (!SetProperty(ref _selectedProvince, value)) return;
                OnPropertyChanged(nameof(HasProvince));
                if (!_reloading && value != null)
                    _catalog.SetActive(value.Id);
            }
        }

        public bool HasProvince => SelectedProvince != null;

        public MenuNodeViewModel SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (SetProperty(ref _selectedNode, value))
                    OnPropertyChanged(nameof(CurrentTask));
            }
        }

        /// <summary>Tác vụ đang hiển thị ở khung bên phải, null nếu chưa chọn.</summary>
        public TaskRunViewModel CurrentTask => SelectedNode?.RunViewModel;

        public string MenuFilter
        {
            get => _menuFilter;
            set
            {
                if (!SetProperty(ref _menuFilter, value)) return;
                foreach (var node in Menu) node.ApplyFilter(value);
            }
        }

        /// <summary>Có tác vụ đang chạy: khoá đổi tỉnh và sửa cấu hình.</summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (!SetProperty(ref _isBusy, value)) return;
                OnPropertyChanged(nameof(CanChangeProvince));
                OnPropertyChanged(nameof(StatusMessage));
                OpenConfigurationCommand.NotifyCanExecuteChanged();
            }
        }

        public bool CanChangeProvince => !IsBusy;

        public string StatusMessage
        {
            get
            {
                var running = _runViewModels.Where(r => r.IsRunning).Select(r => r.Title).ToList();
                return running.Count == 0 ? "Sẵn sàng" : "Đang chạy: " + string.Join(", ", running);
            }
        }

        public string ConfigFolder => _catalog.Folder;

        public RelayCommand OpenConfigurationCommand { get; }

        private TaskRunViewModel CreateRunViewModel(TaskDescriptor task)
        {
            var vm = new TaskRunViewModel(task, () => SelectedProvince, _dialogs);
            vm.RunningChanged += (s, e) =>
            {
                IsBusy = _runViewModels.Any(r => r.IsRunning);
                OnPropertyChanged(nameof(StatusMessage));
            };
            _runViewModels.Add(vm);
            return vm;
        }

        private void OpenConfiguration()
        {
            _dialogs.ShowConfiguration(new ConfigurationViewModel(_catalog, _dialogs));
            ReloadProvinces();
        }

        /// <summary>Đọc lại thư mục cấu hình (sau khi sửa/đổi thư mục).</summary>
        public void ReloadProvinces()
        {
            _reloading = true;
            try
            {
                Provinces.Clear();
                foreach (var p in _catalog.LoadAll())
                    Provinces.Add(p);
                SelectedProvince = _catalog.FindActive(Provinces);
            }
            finally
            {
                _reloading = false;
            }
            OnPropertyChanged(nameof(ConfigFolder));
        }
    }
}
