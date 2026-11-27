using System.Threading;
using System.Threading.Tasks;
using DataTool.App.Models;

namespace DataTool.App.Services
{
    /// <summary>
    /// Kiểm tra kết nối giả lập: chờ một chút rồi báo thành công nếu đã nhập đủ thông tin.
    /// Sẽ thay bằng kiểm tra thật (Oracle, HTTP tới SOLR, RabbitMQ) ở bước Configuration.
    /// </summary>
    public class FakeConnectionTester : IConnectionTester
    {
        public async Task<ConnectionTestOutcome> TestDatabaseAsync(DatabaseSettings s, CancellationToken ct)
        {
            await Task.Delay(900, ct);
            if (string.IsNullOrWhiteSpace(s.Host) || string.IsNullOrWhiteSpace(s.ServiceName))
                return new ConnectionTestOutcome(false, "Chưa nhập Host hoặc Service name.");
            if (string.IsNullOrWhiteSpace(s.UserName))
                return new ConnectionTestOutcome(false, "Chưa nhập tài khoản.");
            return new ConnectionTestOutcome(true, $"Kết nối thành công tới {s.Host}:{s.Port}/{s.ServiceName} (giả lập).");
        }

        public async Task<ConnectionTestOutcome> TestSolrAsync(SolrSettings s, CancellationToken ct)
        {
            await Task.Delay(700, ct);
            if (string.IsNullOrWhiteSpace(s.Url))
                return new ConnectionTestOutcome(false, "Chưa nhập URL.");
            return new ConnectionTestOutcome(true, $"SOLR phản hồi tại {s.Url} (giả lập).");
        }

        public async Task<ConnectionTestOutcome> TestRabbitMqAsync(RabbitMqSettings s, CancellationToken ct)
        {
            await Task.Delay(800, ct);
            if (string.IsNullOrWhiteSpace(s.Host))
                return new ConnectionTestOutcome(false, "Chưa nhập Host.");
            return new ConnectionTestOutcome(true, $"Kết nối thành công tới {s.Host}:{s.Port}{s.VirtualHost} (giả lập).");
        }
    }
}
