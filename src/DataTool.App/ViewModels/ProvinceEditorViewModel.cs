using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using DataTool.Core.Provinces;

namespace DataTool.App.ViewModels
{
    /// <summary>Form sửa một file cấu hình tỉnh. Sửa trên bản sao, chỉ ghi file khi bấm Lưu.</summary>
    public class ProvinceEditorViewModel : ObservableObject
    {
        private ProvinceProfile _saved;
        private ProvinceProfile _current;
        private bool _isActive;

        public ProvinceEditorViewModel(ProvinceProfile profile)
        {
            Load(profile);
        }

        public string Id => _current.Id;
        public string FilePath => _current.FilePath;
        public string DisplayName => _current.DisplayName;

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public bool IsDirty => !_current.ToPairs().SequenceEqual(_saved.ToPairs());

        public IReadOnlyList<string> RepositoryBackendOptions { get; } = new[] { "VDFS", "GridFS" };

        /// <summary>Các khoá lạ (không thuộc danh sách chuẩn) để vẫn sửa được.</summary>
        public ObservableCollection<ExtraFieldViewModel> ExtraFields { get; } = new ObservableCollection<ExtraFieldViewModel>();

        public bool HasExtraFields => ExtraFields.Count > 0;

        public string ServiceKey
        {
            get => _current.ServiceKey;
            set { if (Set(ProvinceKeys.ServiceKey, value)) OnPropertyChanged(nameof(DisplayName)); }
        }

        public string RepositoryBackend { get => _current.RepositoryBackend; set => Set(ProvinceKeys.RepositoryBackend, value); }
        public string SolrUri { get => _current.SolrUri; set => Set(ProvinceKeys.SolrUri, value); }
        public string SolrKey { get => _current.SolrKey; set => Set(ProvinceKeys.SolrKey, value); }
        public string VdmsConnectionString { get => _current.VdmsConnectionString; set => Set(ProvinceKeys.VdmsConnectionString, value); }
        public string LogConnectionString { get => _current.LogConnectionString; set => Set(ProvinceKeys.LogConnectionString, value); }
        public string VdfsApiEndpoint { get => _current.VdfsApiEndpoint; set => Set(ProvinceKeys.VdfsApiEndpoint, value); }
        public string VdfsTag { get => _current.VdfsTag; set => Set(ProvinceKeys.VdfsTag, value); }
        public string VdfsRabbitUri { get => _current.VdfsRabbitUri; set => Set(ProvinceKeys.VdfsRabbitUri, value); }
        public string VdfsExchange { get => _current.VdfsExchange; set => Set(ProvinceKeys.VdfsExchange, value); }
        public string RepoEndpoint { get => _current.RepoEndpoint; set => Set(ProvinceKeys.RepoEndpoint, value); }
        public string RabbitUri { get => _current.RabbitUri; set => Set(ProvinceKeys.RabbitUri, value); }

        public bool IsNewRepo
        {
            get => _current.IsNewRepo;
            set => Set(ProvinceKeys.IsNewRepo, value ? "true" : "false");
        }

        /// <summary>Bản hiện tại để ghi file.</summary>
        public ProvinceProfile ToModel() => _current.Clone();

        /// <summary>Gọi sau khi ghi file thành công.</summary>
        public void MarkSaved(ProvinceProfile savedProfile) => Load(savedProfile);

        /// <summary>Bỏ các thay đổi chưa lưu.</summary>
        public void Reset() => Load(_saved);

        private void Load(ProvinceProfile profile)
        {
            _saved = profile.Clone();
            _current = profile.Clone();

            ExtraFields.Clear();
            foreach (var key in _current.ExtraKeys)
                ExtraFields.Add(new ExtraFieldViewModel(key, _current.Get(key), v => Set(key, v, nameof(ExtraFields))));

            OnPropertyChanged(string.Empty); // làm mới mọi binding
        }

        private bool Set(string key, string value, [CallerMemberName] string propertyName = null)
        {
            value = value ?? string.Empty;
            if (_current.Get(key) == value && _current.Contains(key)) return false;
            _current.Set(key, value);
            OnPropertyChanged(propertyName);
            OnPropertyChanged(nameof(IsDirty));
            return true;
        }
    }

    public class ExtraFieldViewModel : ObservableObject
    {
        private readonly System.Action<string> _onChanged;
        private string _value;

        public ExtraFieldViewModel(string key, string value, System.Action<string> onChanged)
        {
            Key = key;
            _value = value;
            _onChanged = onChanged;
        }

        public string Key { get; }

        public string Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                    _onChanged(value);
            }
        }
    }
}
