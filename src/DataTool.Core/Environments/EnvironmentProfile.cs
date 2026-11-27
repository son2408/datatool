using System;

namespace DataTool.Core.Environments
{
    public enum EnvironmentKind
    {
        Development,
        Test,
        Production,
        Other
    }

    /// <summary>Một bộ cấu hình môi trường: CSDL, SOLR, RabbitMQ. Mật khẩu ở dạng rõ trong bộ nhớ.</summary>
    public class EnvironmentProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public EnvironmentKind Kind { get; set; } = EnvironmentKind.Development;
        public DatabaseSettings Database { get; set; } = new DatabaseSettings();
        public SolrSettings Solr { get; set; } = new SolrSettings();
        public RabbitMqSettings RabbitMq { get; set; } = new RabbitMqSettings();

        public bool IsProduction => Kind == EnvironmentKind.Production;

        public EnvironmentProfile Clone() => new EnvironmentProfile
        {
            Id = Id,
            Name = Name,
            Kind = Kind,
            Database = Database?.Clone() ?? new DatabaseSettings(),
            Solr = Solr?.Clone() ?? new SolrSettings(),
            RabbitMq = RabbitMq?.Clone() ?? new RabbitMqSettings()
        };

        public override string ToString() => $"{Name} ({Kind})";
    }

    public class DatabaseSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 1521;
        public string ServiceName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        /// <summary>Chuỗi kết nối Oracle (EZ Connect).</summary>
        public string ToOracleConnectionString(int connectionTimeoutSeconds = 15) =>
            $"User Id={UserName};Password={Password};Data Source={Host}:{Port}/{ServiceName};Connection Timeout={connectionTimeoutSeconds}";

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
