using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTool.App.Models;
using DataTool.App.Services;
using DataTool.UI.Controls;

namespace DataTool.App.ViewModels
{
    /// <summary>Cửa sổ quản lý các bộ cấu hình (thêm, nhân bản, xoá, lưu).</summary>
    public class ConfigurationViewModel : ObservableObject
    {
        private readonly IProfileStore _store;
        private readonly IConnectionTester _tester;
        private readonly IDialogService _dialogs;
        private ProfileEditorViewModel _selectedProfile;

        public ConfigurationViewModel(IProfileStore store, IConnectionTester tester, IDialogService dialogs)
        {
            _store = store;
            _tester = tester;
            _dialogs = dialogs;

            foreach (var profile in store.Load())
                Profiles.Add(new ProfileEditorViewModel(profile, tester));
            SelectedProfile = Profiles.FirstOrDefault();

            AddCommand = new RelayCommand(Add);
            DuplicateCommand = new RelayCommand(Duplicate, () => SelectedProfile != null);
            DeleteCommand = new RelayCommand(Delete, () => SelectedProfile != null);
            SaveCommand = new RelayCommand(Save);
        }

        /// <summary>Yêu cầu View đóng cửa sổ; tham số là true nếu đã lưu.</summary>
        public event EventHandler<bool> CloseRequested;

        public ObservableCollection<ProfileEditorViewModel> Profiles { get; } = new ObservableCollection<ProfileEditorViewModel>();

        public IReadOnlyList<EnvironmentLevel> Levels { get; } =
            (EnvironmentLevel[])Enum.GetValues(typeof(EnvironmentLevel));

        public ProfileEditorViewModel SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (!SetProperty(ref _selectedProfile, value)) return;
                OnPropertyChanged(nameof(HasSelection));
                DuplicateCommand?.NotifyCanExecuteChanged();
                DeleteCommand?.NotifyCanExecuteChanged();
            }
        }

        public bool HasSelection => SelectedProfile != null;

        public string ConfigFilePath => AppPaths.ConfigFile;

        public RelayCommand AddCommand { get; }
        public RelayCommand DuplicateCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }

        private void Add()
        {
            var profile = new EnvironmentProfile { Name = NextName("Cấu hình mới"), Level = EnvironmentLevel.Development };
            var vm = new ProfileEditorViewModel(profile, _tester);
            Profiles.Add(vm);
            SelectedProfile = vm;
        }

        private void Duplicate()
        {
            var copy = SelectedProfile.CreateCopy(NextName(SelectedProfile.Name + " (bản sao)"));
            var vm = new ProfileEditorViewModel(copy, _tester);
            Profiles.Insert(Profiles.IndexOf(SelectedProfile) + 1, vm);
            SelectedProfile = vm;
        }

        private void Delete()
        {
            var target = SelectedProfile;
            bool ok = target.IsProduction
                ? _dialogs.ConfirmDanger("Xoá cấu hình Production",
                    $"Bạn sắp xoá bộ cấu hình \"{target.Name}\". Thay đổi có hiệu lực khi bấm Lưu.",
                    target.Name, "Xoá")
                : _dialogs.Confirm("Xoá cấu hình",
                    $"Xoá bộ cấu hình \"{target.Name}\"? Thay đổi có hiệu lực khi bấm Lưu.", "Xoá");
            if (!ok) return;

            int index = Profiles.IndexOf(target);
            Profiles.Remove(target);
            SelectedProfile = Profiles.Count == 0 ? null : Profiles[Math.Min(index, Profiles.Count - 1)];
        }

        private void Save()
        {
            var error = Validate();
            if (error != null)
            {
                _dialogs.ShowError("Chưa lưu được", error);
                return;
            }

            _store.Save(Profiles.Select(p => p.ToModel()));
            CloseRequested?.Invoke(this, true);
        }

        private string Validate()
        {
            var empty = Profiles.FirstOrDefault(p => string.IsNullOrWhiteSpace(p.Name));
            if (empty != null)
            {
                SelectedProfile = empty;
                return "Tên cấu hình không được để trống.";
            }

            var duplicate = Profiles
                .GroupBy(p => p.Name.Trim(), StringComparer.CurrentCultureIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null)
                return $"Có nhiều cấu hình cùng tên \"{duplicate.Key}\". Hãy đặt tên khác nhau.";

            return null;
        }

        private string NextName(string baseName)
        {
            string name = baseName;
            int i = 2;
            while (Profiles.Any(p => string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase)))
                name = $"{baseName} {i++}";
            return name;
        }
    }
}
