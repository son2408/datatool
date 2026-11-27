using System;
using System.Configuration;
using System.Reflection;
using DataTool.Core.Provinces;

namespace DataTool.App.Vbdlis
{
    /// <summary>
    /// Thư viện cũ (VDMSDataHelper...) đọc ConfigurationManager.AppSettings / ConnectionStrings.
    /// Lớp này ghi đè các giá trị đó (chỉ trong bộ nhớ, không sửa file) bằng cấu hình tỉnh đang chọn,
    /// để đổi tỉnh không cần sửa app.config.
    /// </summary>
    public static class AppConfigOverride
    {
        public static void Apply(ProvinceProfile p)
        {
            SetAppSetting("SolrKey", p.SolrKey);
            SetAppSetting("RepositoryBackend", p.RepositoryBackend);
            SetAppSetting("VDFS.ApiEndpoint", p.VdfsApiEndpoint);
            SetAppSetting("VDFS.Tag", p.VdfsTag);
            SetAppSetting("VDFS.RabbitUri", p.VdfsRabbitUri);
            SetAppSetting("VDFS.Exchange", p.VdfsExchange);

            SetConnectionString("VDMSConnectionString", p.VdmsConnectionString);
            SetConnectionString("LogConnectionString", p.LogConnectionString);
        }

        public static void SetAppSetting(string key, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            // AppSettings là NameValueCollection chỉ đọc: mở khoá rồi ghi.
            var settings = ConfigurationManager.AppSettings;
            var readOnly = typeof(System.Collections.Specialized.NameObjectCollectionBase)
                .GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            readOnly?.SetValue(settings, false, null);
            settings[key] = value;
            readOnly?.SetValue(settings, true, null);
        }

        public static void SetConnectionString(string name, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            var collection = ConfigurationManager.ConnectionStrings;
            var collectionReadOnly = typeof(ConfigurationElementCollection)
                .GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            var elementReadOnly = typeof(ConfigurationElement)
                .GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

            collectionReadOnly?.SetValue(collection, false);
            var existing = collection[name];
            if (existing == null)
            {
                collection.Add(new ConnectionStringSettings(name, value));
            }
            else
            {
                elementReadOnly?.SetValue(existing, false);
                existing.ConnectionString = value;
            }
        }
    }
}
