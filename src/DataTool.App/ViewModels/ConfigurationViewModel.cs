using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTool.App.Services;
using DataTool.Core.Provinces;

namespace DataTool.App.ViewModels
{
    /// <summary>
    /// Cửa sổ cấu hình: danh sách tỉnh (mỗi tỉnh một file .yml trong thư mục), chọn 1 tỉnh đang dùng,
    /// bấm vào tỉnh để xem và sửa.
    /// </summary>
    public class ConfigurationViewModel : ObservableObject
    {
        private readonly ProvinceCatalog _catalog;
        private readonly IDialogService _dialogs;
        private ProvinceEditorViewModel _selected;
        private string _filter = string.Empty;

        public ConfigurationViewModel(ProvinceCatalog catalog, IDialogService dialogs)
        {
            _catalog = catalog;
            _dialogs = dialogs;

            ProvincesView = CollectionViewSource.GetDefaultView(Provinces);
            ProvincesView.Filter = o => Matches((ProvinceEditorViewModel)o);

            ChangeFolderCommand = new RelayCommand(ChangeFolder);
            ReloadCommand = new RelayCommand(() => { if (ConfirmDiscardAll()) Reload(); });
            AddCommand = new RelayCommand(Add);
            DuplicateCommand = new RelayCommand(Duplicate, () => Selected != null);
            DeleteCommand = new RelayCommand(Delete, () => Selected != null);
            OpenFileCommand = new RelayCommand(() => _dialogs.OpenFile(Selected.FilePath), () => Selected?.FilePath != null);
            UseCommand = new RelayCommand(UseSelected, () => Selected != null && !Selected.IsActive);
            SaveCommand = new RelayCommand(SaveSelected, () => Selected?.IsDirty == true);
            DiscardCommand = new RelayCommand(() => Selected?.Reset(), () => Selected?.IsDirty == true);

            Reload();
        }

        public ObservableCollection<ProvinceEditorViewModel> Provinces { get; } = new ObservableCollection<ProvinceEditorViewModel>();
        public ICollectionView ProvincesView { get; }

        public string Folder => _catalog.Folder;

        public string CountText => $"{Provinces.Count} TỈNH";

        public bool IsEmpty => Provinces.Count == 0;

        public string Filter
        {
            get => _filter;
            set
            {
                if (SetProperty(ref _filter, value))
                    ProvincesView.Refresh();
            }
        }

        public ProvinceEditorViewModel Selected
        {
            get => _selected;
            set
            {
                var old = _selected;
                if (!SetProperty(ref _selected, value)) return;
                if (old != null) old.PropertyChanged -= OnSelectedChanged;
                if (value != null) value.PropertyChanged += OnSelectedChanged;
                OnPropertyChanged(nameof(HasSelection));
                RefreshCommands();
            }
        }

        public bool HasSelection => Selected != null;

        public RelayCommand ChangeFolderCommand { get; }
        public RelayCommand ReloadCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand DuplicateCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand OpenFileCommand { get; }
        public RelayCommand UseCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DiscardCommand { get; }

        /// <summary>Gọi khi người dùng đóng cửa sổ. Trả về false để giữ cửa sổ mở.</summary>
        public bool CanClose() => ConfirmDiscardAll();

        private void Reload()
        {
            string keepId = Selected?.Id;
            Provinces.Clear();
            foreach (var profile in _catalog.LoadAll())
                Provinces.Add(new ProvinceEditorViewModel(profile) { IsActive = profile.Id == _catalog.ActiveId });

            Selected = Provinces.FirstOrDefault(p => p.Id == keepId)
                ?? Provinces.FirstOrDefault(p => p.IsActive)
                ?? Provinces.FirstOrDefault();

            OnPropertyChanged(nameof(Folder));
            OnCountChanged();

            if (_catalog.LastErrors.Count > 0)
                _dialogs.ShowError("Một số file không đọc được", string.Join(Environment.NewLine, _catalog.LastErrors));
        }

        private void ChangeFolder()
        {
            if (!ConfirmDiscardAll()) return;
            var folder = _dialogs.PickFolder("Chọn thư mục chứa các file cấu hình tỉnh (.yml)", Folder);
            if (string.IsNullOrEmpty(folder)) return;
            _catalog.ChangeFolder(folder);
            Reload();
        }

        private void Add()
        {
            var store = _catalog.Store;
            var id = _dialogs.Prompt("Thêm tỉnh", "Nhập tên file cấu hình (không cần đuôi .yml), ví dụ gla_64.", "",
                store.ValidateNewIdMessage);
            if (id == null) return;
            AddEditor(TryCreate(() => store.Create(id)));
        }

        private void Duplicate()
        {
            var store = _catalog.Store;
            var source = Selected.ToModel();
            var id = _dialogs.Prompt("Nhân bản cấu hình", $"Tạo file mới từ cấu hình {Selected.DisplayName}. Nhập tên file mới:",
                Selected.Id + "_copy", store.ValidateNewIdMessage);
            if (id == null) return;
            AddEditor(TryCreate(() => store.Create(id, source)));
        }

        private void Delete()
        {
            var target = Selected;
            bool ok = _dialogs.ConfirmDanger("Xoá cấu hình tỉnh",
                $"File {target.Id}.yml sẽ được chuyển vào thư mục _deleted trong thư mục cấu hình.",
                target.DisplayName, "Xoá");
            if (!ok) return;

            try
            {
                _catalog.Store.Delete(target.ToModel());
            }
            catch (Exception ex)
            {
                _dialogs.ShowError("Không xoá được", ex.Message);
                return;
            }

            if (target.IsActive) _catalog.SetActive(null);
            int index = Provinces.IndexOf(target);
            Provinces.Remove(target);
            Selected = Provinces.Count == 0 ? null : Provinces[Math.Min(index, Provinces.Count - 1)];
            OnCountChanged();
        }

        private void UseSelected()
        {
            if (Selected.IsDirty)
            {
                if (!_dialogs.Confirm("Lưu thay đổi", $"Cấu hình {Selected.DisplayName} có thay đổi chưa lưu. Lưu rồi dùng cấu hình này?", "Lưu và dùng"))
                    return;
                if (!Save(Selected)) return;
            }

            foreach (var p in Provinces) p.IsActive = p == Selected;
            _catalog.SetActive(Selected.Id);
            RefreshCommands();
        }

        private void SaveSelected() => Save(Selected);

        private bool Save(ProvinceEditorViewModel editor)
        {
            try
            {
                var model = editor.ToModel();
                _catalog.Store.Save(model);
                editor.MarkSaved(model);
                RefreshCommands();
                return true;
            }
            catch (Exception ex)
            {
                _dialogs.ShowError("Không lưu được", ex.Message);
                return false;
            }
        }

        private ProvinceProfile TryCreate(Func<ProvinceProfile> create)
        {
            try
            {
                return create();
            }
            catch (Exception ex)
            {
                _dialogs.ShowError("Không tạo được file", ex.Message);
                return null;
            }
        }

        private void AddEditor(ProvinceProfile profile)
        {
            if (profile == null) return;
            var editor = new ProvinceEditorViewModel(profile);
            Provinces.Add(editor);
            Filter = string.Empty;
            Selected = editor;
            OnCountChanged();
        }

        private bool ConfirmDiscardAll()
        {
            var dirty = Provinces.Where(p => p.IsDirty).Select(p => p.DisplayName).ToList();
            if (dirty.Count == 0) return true;
            return _dialogs.Confirm("Bỏ thay đổi chưa lưu?",
                $"Các cấu hình sau có thay đổi chưa lưu: {string.Join(", ", dirty)}.\nTiếp tục sẽ bỏ các thay đổi này.",
                "Bỏ thay đổi");
        }

        private bool Matches(ProvinceEditorViewModel p)
        {
            if (string.IsNullOrWhiteSpace(Filter)) return true;
            string f = Filter.Trim();
            return p.DisplayName.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0
                || p.Id.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void OnSelectedChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProvinceEditorViewModel.IsDirty) || string.IsNullOrEmpty(e.PropertyName))
                RefreshCommands();
        }

        private void OnCountChanged()
        {
            OnPropertyChanged(nameof(CountText));
            OnPropertyChanged(nameof(IsEmpty));
        }

        private void RefreshCommands()
        {
            DuplicateCommand?.NotifyCanExecuteChanged();
            DeleteCommand?.NotifyCanExecuteChanged();
            OpenFileCommand?.NotifyCanExecuteChanged();
            UseCommand?.NotifyCanExecuteChanged();
            SaveCommand?.NotifyCanExecuteChanged();
            DiscardCommand?.NotifyCanExecuteChanged();
        }
    }
}
