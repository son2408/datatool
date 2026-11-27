using System.Collections.Generic;
using System.Linq;
using DataTool.Configuration;
using DataTool.Core.Provinces;

namespace DataTool.App.Services
{
    /// <summary>
    /// Nơi App lấy danh sách tỉnh và tỉnh đang dùng. Ghi nhớ thư mục cấu hình và tỉnh đang dùng
    /// vào settings.yml để lần mở sau vẫn giữ.
    /// </summary>
    public class ProvinceCatalog
    {
        private readonly UserSettingsStore _settingsStore;
        private readonly string _defaultFolder;
        private readonly UserSettings _settings;

        public ProvinceCatalog(UserSettingsStore settingsStore, string defaultFolder)
        {
            _settingsStore = settingsStore;
            _defaultFolder = defaultFolder;
            _settings = settingsStore.Load();
        }

        public string Folder => string.IsNullOrWhiteSpace(_settings.ConfigFolder) ? _defaultFolder : _settings.ConfigFolder;

        public string ActiveId => _settings.ActiveProvince;

        public ProvinceStore Store => new ProvinceStore(Folder);

        public IReadOnlyList<string> LastErrors { get; private set; } = new string[0];

        public IList<ProvinceProfile> LoadAll()
        {
            var store = Store;
            var list = store.LoadAll();
            LastErrors = store.LoadErrors;
            return list;
        }

        public void ChangeFolder(string folder)
        {
            _settings.ConfigFolder = folder;
            _settingsStore.Save(_settings);
        }

        public void SetActive(string id)
        {
            if (_settings.ActiveProvince == id) return;
            _settings.ActiveProvince = id;
            _settingsStore.Save(_settings);
        }

        /// <summary>Tỉnh đang dùng trong danh sách; nếu chưa chọn hoặc không còn thì null.</summary>
        public ProvinceProfile FindActive(IEnumerable<ProvinceProfile> list) =>
            string.IsNullOrEmpty(ActiveId) ? null : list.FirstOrDefault(p => p.Id == ActiveId);
    }
}
