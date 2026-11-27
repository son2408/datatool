using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataTool.Core.Provinces;

namespace DataTool.Configuration
{
    /// <summary>Thư mục chứa các file cấu hình tỉnh (*.yml), mỗi file một tỉnh.</summary>
    public class ProvinceStore
    {
        private static readonly Regex ValidId = new Regex(@"^[A-Za-z0-9_\-]+$", RegexOptions.Compiled);
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        public ProvinceStore(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentException("Chưa chọn thư mục cấu hình.", nameof(folder));
            Folder = Path.GetFullPath(folder);
        }

        public string Folder { get; }

        /// <summary>Lỗi đọc file ở lần LoadAll gần nhất (file hỏng không làm dừng việc đọc các file khác).</summary>
        public IReadOnlyList<string> LoadErrors { get; private set; } = new string[0];

        public IList<ProvinceProfile> LoadAll()
        {
            var errors = new List<string>();
            var result = new List<ProvinceProfile>();
            if (!Directory.Exists(Folder))
            {
                LoadErrors = errors;
                return result;
            }

            var files = Directory.GetFiles(Folder, "*.yml").Concat(Directory.GetFiles(Folder, "*.yaml"));
            foreach (var file in files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    result.Add(Load(file));
                }
                catch (Exception ex)
                {
                    errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
                }
            }

            LoadErrors = errors;
            return result.OrderBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public ProvinceProfile Load(string filePath)
        {
            string text = File.ReadAllText(filePath, Utf8);
            return new ProvinceProfile(Path.GetFileNameWithoutExtension(filePath), ProvinceYaml.Parse(text)) { FilePath = filePath };
        }

        /// <summary>Ghi lại file, giữ nguyên comment và thứ tự khoá của file gốc.</summary>
        public void Save(ProvinceProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            string path = profile.FilePath ?? PathFor(profile.Id);
            string text = File.Exists(path)
                ? ProvinceYaml.Update(File.ReadAllText(path, Utf8), profile.ToPairs())
                : ProvinceYaml.CreateTemplate(profile.ToPairs());

            WriteAtomic(path, text);
            profile.FilePath = path;
        }

        /// <summary>Tạo file mới (lỗi nếu đã có file cùng tên).</summary>
        public ProvinceProfile Create(string id, ProvinceProfile copyFrom = null)
        {
            ValidateNewId(id);
            var profile = copyFrom != null
                ? copyFrom.Clone(id)
                : new ProvinceProfile(id, ProvinceKeys.All.Select(k => new KeyValuePair<string, string>(k, DefaultValue(k))));
            profile.FilePath = null;

            Directory.CreateDirectory(Folder);
            string path = PathFor(id);
            WriteAtomic(path, ProvinceYaml.CreateTemplate(profile.ToPairs()));
            profile.FilePath = path;
            return profile;
        }

        /// <summary>Không xoá hẳn: chuyển file vào thư mục con "_deleted" để có thể lấy lại.</summary>
        public string Delete(ProvinceProfile profile)
        {
            string path = profile.FilePath ?? PathFor(profile.Id);
            if (!File.Exists(path)) return null;

            string trash = Path.Combine(Folder, "_deleted");
            Directory.CreateDirectory(trash);
            string target = Path.Combine(trash, $"{Path.GetFileNameWithoutExtension(path)}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(path)}");
            File.Move(path, target);
            return target;
        }

        public string ValidateNewIdMessage(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "Tên file không được trống.";
            if (!ValidId.IsMatch(id)) return "Tên file chỉ gồm chữ không dấu, số, dấu gạch dưới hoặc gạch ngang (ví dụ gla_64).";
            if (File.Exists(PathFor(id)) || File.Exists(Path.Combine(Folder, id + ".yaml"))) return $"Đã có file {id}.yml trong thư mục.";
            return null;
        }

        private void ValidateNewId(string id)
        {
            var message = ValidateNewIdMessage(id);
            if (message != null) throw new InvalidOperationException(message);
        }

        private string PathFor(string id) => Path.Combine(Folder, id + ".yml");

        private static string DefaultValue(string key)
        {
            switch (key)
            {
                case ProvinceKeys.RepositoryBackend: return "VDFS";
                case ProvinceKeys.VdfsTag: return "lis";
                case ProvinceKeys.VdfsExchange: return "vdfs";
                case ProvinceKeys.IsNewRepo: return "false";
                default: return string.Empty;
            }
        }

        private static void WriteAtomic(string path, string text)
        {
            string temp = path + ".tmp";
            File.WriteAllText(temp, text, Utf8);
            if (File.Exists(path))
                File.Replace(temp, path, null);
            else
                File.Move(temp, path);
        }
    }
}
