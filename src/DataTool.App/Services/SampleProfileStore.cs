using System.Collections.Generic;
using System.Linq;
using DataTool.App.Models;
using DataTool.UI.Controls;

namespace DataTool.App.Services
{
    /// <summary>
    /// Kho cấu hình trong bộ nhớ với 3 môi trường mẫu, chỉ dùng khi dựng giao diện.
    /// Sẽ thay bằng kho JSON có mã hoá mật khẩu ở bước Configuration.
    /// </summary>
    public class SampleProfileStore : IProfileStore
    {
        private List<EnvironmentProfile> _profiles = new List<EnvironmentProfile>
        {
            new EnvironmentProfile
            {
                Name = "Dev",
                Level = EnvironmentLevel.Development,
                Database = new DatabaseSettings { Host = "localhost", Port = 1521, ServiceName = "VBDLISDEV", UserName = "vbdlis_dev" },
                Solr = new SolrSettings { Url = "http://localhost:8983/solr", Core = "vbdlis" },
                RabbitMq = new RabbitMqSettings { Host = "localhost", UserName = "guest", Password = "guest" }
            },
            new EnvironmentProfile
            {
                Name = "Test",
                Level = EnvironmentLevel.Test,
                Database = new DatabaseSettings { Host = "10.0.0.20", Port = 1521, ServiceName = "VBDLISTEST", UserName = "vbdlis_test" },
                Solr = new SolrSettings { Url = "http://10.0.0.21:8983/solr", Core = "vbdlis" },
                RabbitMq = new RabbitMqSettings { Host = "10.0.0.22", UserName = "vbdlis" }
            },
            new EnvironmentProfile
            {
                Name = "Production",
                Level = EnvironmentLevel.Production,
                Database = new DatabaseSettings { Host = "db.prod.local", Port = 1521, ServiceName = "VBDLIS", UserName = "vbdlis" },
                Solr = new SolrSettings { Url = "http://solr.prod.local:8983/solr", Core = "vbdlis" },
                RabbitMq = new RabbitMqSettings { Host = "mq.prod.local", UserName = "vbdlis" }
            }
        };

        public IList<EnvironmentProfile> Load() => _profiles.Select(p => p.Clone()).ToList();

        public void Save(IEnumerable<EnvironmentProfile> profiles) => _profiles = profiles.Select(p => p.Clone()).ToList();
    }
}
