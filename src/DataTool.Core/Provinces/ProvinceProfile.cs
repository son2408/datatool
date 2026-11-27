using System;
using System.Collections.Generic;
using System.Linq;

namespace DataTool.Core.Provinces
{
    /// <summary>Tên các khoá trong file cấu hình tỉnh (.yml).</summary>
    public static class ProvinceKeys
    {
        public const string ServiceKey = "service_key";
        public const string RepositoryBackend = "repository_backend";
        public const string SolrUri = "solr_uri";
        public const string SolrKey = "solr_key";
        public const string VdmsConnectionString = "vdms_connection_string";
        public const string LogConnectionString = "log_connection_string";
        public const string VdfsApiEndpoint = "vdfs_api_endpoint";
        public const string VdfsTag = "vdfs_tag";
        public const string VdfsRabbitUri = "vdfs_rabbit_uri";
        public const string VdfsExchange = "vdfs_exchange";
        public const string RepoEndpoint = "repo_endpoint";
        public const string IsNewRepo = "is_new_repo";
        public const string RabbitUri = "rabbit_uri";

        /// <summary>Các khoá đã biết, theo thứ tự trong file mẫu.</summary>
        public static readonly IReadOnlyList<string> All = new[]
        {
            ServiceKey, RepositoryBackend,
            SolrUri, SolrKey,
            VdmsConnectionString, LogConnectionString,
            VdfsApiEndpoint, VdfsTag, VdfsRabbitUri, VdfsExchange,
            RepoEndpoint, IsNewRepo,
            RabbitUri
        };
    }

    /// <summary>
    /// Cấu hình của một tỉnh, đọc từ một file .yml. Mọi giá trị được giữ dạng chuỗi theo khoá
    /// (kể cả khoá lạ không có trong <see cref="ProvinceKeys"/>), thuộc tính có kiểu chỉ là lối tắt.
    /// </summary>
    public class ProvinceProfile
    {
        private readonly Dictionary<string, string> _values;
        private readonly List<string> _order;

        public ProvinceProfile(string id, IEnumerable<KeyValuePair<string, string>> values = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            _values = new Dictionary<string, string>(StringComparer.Ordinal);
            _order = new List<string>();
            if (values != null)
                foreach (var kv in values) Set(kv.Key, kv.Value);
        }

        /// <summary>Tên file không có phần mở rộng, ví dụ "gla_64".</summary>
        public string Id { get; }

        /// <summary>Đường dẫn file .yml (null nếu chưa lưu).</summary>
        public string FilePath { get; set; }

        /// <summary>Tên hiển thị: service_key, nếu trống thì dùng Id.</summary>
        public string DisplayName => string.IsNullOrWhiteSpace(ServiceKey) ? Id : ServiceKey;

        /// <summary>Các khoá theo thứ tự xuất hiện.</summary>
        public IReadOnlyList<string> Keys => _order;

        /// <summary>Các khoá không nằm trong danh sách đã biết.</summary>
        public IEnumerable<string> ExtraKeys => _order.Where(k => !ProvinceKeys.All.Contains(k));

        public string Get(string key) => key != null && _values.TryGetValue(key, out var v) ? v : string.Empty;

        public void Set(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Khoá không được trống.", nameof(key));
            if (!_values.ContainsKey(key)) _order.Add(key);
            _values[key] = value ?? string.Empty;
        }

        public bool Contains(string key) => _values.ContainsKey(key);

        public IEnumerable<KeyValuePair<string, string>> ToPairs() => _order.Select(k => new KeyValuePair<string, string>(k, _values[k]));

        public ProvinceProfile Clone(string newId = null) => new ProvinceProfile(newId ?? Id, ToPairs()) { FilePath = newId == null ? FilePath : null };

        public string ServiceKey { get => Get(ProvinceKeys.ServiceKey); set => Set(ProvinceKeys.ServiceKey, value); }
        public string RepositoryBackend { get => Get(ProvinceKeys.RepositoryBackend); set => Set(ProvinceKeys.RepositoryBackend, value); }
        public string SolrUri { get => Get(ProvinceKeys.SolrUri); set => Set(ProvinceKeys.SolrUri, value); }
        public string SolrKey { get => Get(ProvinceKeys.SolrKey); set => Set(ProvinceKeys.SolrKey, value); }
        public string VdmsConnectionString { get => Get(ProvinceKeys.VdmsConnectionString); set => Set(ProvinceKeys.VdmsConnectionString, value); }
        public string LogConnectionString { get => Get(ProvinceKeys.LogConnectionString); set => Set(ProvinceKeys.LogConnectionString, value); }
        public string VdfsApiEndpoint { get => Get(ProvinceKeys.VdfsApiEndpoint); set => Set(ProvinceKeys.VdfsApiEndpoint, value); }
        public string VdfsTag { get => Get(ProvinceKeys.VdfsTag); set => Set(ProvinceKeys.VdfsTag, value); }
        public string VdfsRabbitUri { get => Get(ProvinceKeys.VdfsRabbitUri); set => Set(ProvinceKeys.VdfsRabbitUri, value); }
        public string VdfsExchange { get => Get(ProvinceKeys.VdfsExchange); set => Set(ProvinceKeys.VdfsExchange, value); }
        public string RepoEndpoint { get => Get(ProvinceKeys.RepoEndpoint); set => Set(ProvinceKeys.RepoEndpoint, value); }
        public string RabbitUri { get => Get(ProvinceKeys.RabbitUri); set => Set(ProvinceKeys.RabbitUri, value); }

        public bool IsNewRepo
        {
            get => bool.TryParse(Get(ProvinceKeys.IsNewRepo), out var b) && b;
            set => Set(ProvinceKeys.IsNewRepo, value ? "true" : "false");
        }

        public override string ToString() => DisplayName;
    }
}
