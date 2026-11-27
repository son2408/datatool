using System;
using DataTool.UI.Controls;

namespace DataTool.App.Models
{
    // TẠM THỜI cho giai đoạn dựng giao diện.
    // Ở bước Configuration, các model này sẽ chuyển sang project DataTool.Configuration
    // (lưu JSON, mã hoá mật khẩu) và App chỉ ánh xạ sang ViewModel.

    public class EnvironmentProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public EnvironmentLevel Level { get; set; } = EnvironmentLevel.Development;
        public DatabaseSettings Database { get; set; } = new DatabaseSettings();
        public SolrSettings Solr { get; set; } = new SolrSettings();
        public RabbitMqSettings RabbitMq { get; set; } = new RabbitMqSettings();

        public bool IsProduction => Level == EnvironmentLevel.Production;

        public EnvironmentProfile Clone() => new EnvironmentProfile
        {
            Id = Id,
            Name = Name,
            Level = Level,
            Database = Database.Clone(),
            Solr = Solr.Clone(),
            RabbitMq = RabbitMq.Clone()
        };
    }

    public class DatabaseSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 1521;
        public string ServiceName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public DatabaseSettings Clone() => (DatabaseSettings)MemberwiseClone();
    }

    public class SolrSettings
    {
        public string Url { get; set; } = string.Empty;
        public string Core { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public SolrSettings Clone() => (SolrSettings)MemberwiseClone();
    }

    public class RabbitMqSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 5672;
        public string VirtualHost { get; set; } = "/";
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public RabbitMqSettings Clone() => (RabbitMqSettings)MemberwiseClone();
    }
}
