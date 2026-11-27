using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTool.App.Models;
using DataTool.App.Services;

namespace DataTool.App.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IProfileStore _profileStore;
        private readonly IConnectionTester _connectionTester;
        private readonly IDialogService _dialogs;
        private readonly List<TaskRunViewModel> _runViewModels = new List<TaskRunViewModel>();

        private EnvironmentProfile _selectedEnvironment;
        private MenuNodeViewModel _selectedNode;
        private string _menuFilter = string.Empty;
        private bool _isBusy;

        public MainViewModel(IProfileStore profileStore, IConnectionTester connectionTester,
            IDialogService dialogs, IEnumerable<TaskDescriptor> tasks)
        {
            _profileStore = profileStore;
            _connectionTester = connectionTester;
            _dialogs = dialogs;

            foreach (var node in MenuNodeViewModel.BuildTree(tasks, CreateRunViewModel))
                Menu.Add(node);

            OpenConfigurationCommand = new RelayCommand(OpenConfiguration, () => !IsBusy);
            ReloadEnvironments(null);
        }

        public ObservableCollection<EnvironmentProfile> Environments { get; } = new ObservableCollection<EnvironmentProfile>();
        public ObservableCollection<MenuNodeViewModel> Menu { get; } = new ObservableCollection<MenuNodeViewModel>();

        public EnvironmentProfile SelectedEnvironment
        {
            get => _selectedEnvironment;
            set
            {
                if (SetProperty(ref _selectedEnvironment, value))
                    OnPropertyChanged(nameof(IsProduction));
            }
        }

        public bool IsProduction => SelectedEnvironment?.IsProduction == true;

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

        /// <summary>Có tác vụ đang chạy: khoá đổi môi trường và sửa cấu hình.</summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (!SetProperty(ref _isBusy, value)) return;
                OnPropertyChanged(nameof(CanChangeEnvironment));
                OnPropertyChanged(nameof(StatusMessage));
                OpenConfigurationCommand.NotifyCanExecuteChanged();
            }
        }

        public bool CanChangeEnvironment => !IsBusy;

        public string StatusMessage
        {
            get
            {
                var running = _runViewModels.Where(r => r.IsRunning).Select(r => r.Title).ToList();
                return running.Count == 0 ? "Sẵn sàng" : "Đang chạy: " + string.Join(", ", running);
            }
        }

        public string DataFolder => AppPaths.DataFolder;

        public RelayCommand OpenConfigurationCommand { get; }

        private TaskRunViewModel CreateRunViewModel(TaskDescriptor task)
        {
            var vm = new TaskRunViewModel(task, () => SelectedEnvironment, _dialogs);
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
            var configVm = new ConfigurationViewModel(_profileStore, _connectionTester, _dialogs);
            if (_dialogs.ShowConfiguration(configVm))
                ReloadEnvironments(SelectedEnvironment?.Id);
        }

        private void ReloadEnvironments(Guid? keepSelectedId)
        {
            Environments.Clear();
            foreach (var profile in _profileStore.Load())
                Environments.Add(profile);

            SelectedEnvironment =
                Environments.FirstOrDefault(e => keepSelectedId.HasValue && e.Id == keepSelectedId.Value)
                ?? Environments.FirstOrDefault(e => !e.IsProduction)
                ?? Environments.FirstOrDefault();
        }
    }
}
