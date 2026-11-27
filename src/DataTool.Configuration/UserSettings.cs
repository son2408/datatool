using System;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DataTool.Configuration
{
    /// <summary>Thiết lập của người dùng: thư mục cấu hình và tỉnh đang dùng.</summary>
    public class UserSettings
    {
        /// <summary>Thư mục chứa các file .yml của tỉnh.</summary>
        public string ConfigFolder { get; set; }

        /// <summary>Id (tên file không đuôi) của tỉnh đang dùng.</summary>
        public string ActiveProvince { get; set; }
    }

    public class UserSettingsStore
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        public UserSettingsStore(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public string FilePath { get; }

        public UserSettings Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new UserSettings();
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
                return deserializer.Deserialize<UserSettings>(File.ReadAllText(FilePath, Utf8)) ?? new UserSettings();
            }
            catch
            {
                // File thiết lập hỏng thì dùng mặc định, không làm hỏng việc mở app.
                return new UserSettings();
            }
        }

        public void Save(UserSettings settings)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FilePath)));
            File.WriteAllText(FilePath, serializer.Serialize(settings), Utf8);
        }
    }
}
