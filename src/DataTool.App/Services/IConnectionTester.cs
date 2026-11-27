using System.Threading;
using System.Threading.Tasks;
using DataTool.App.Models;

namespace DataTool.App.Services
{
    public sealed class ConnectionTestOutcome
    {
        public ConnectionTestOutcome(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; }
        public string Message { get; }
    }

    public interface IConnectionTester
    {
        Task<ConnectionTestOutcome> TestDatabaseAsync(DatabaseSettings settings, CancellationToken cancellationToken);
        Task<ConnectionTestOutcome> TestSolrAsync(SolrSettings settings, CancellationToken cancellationToken);
        Task<ConnectionTestOutcome> TestRabbitMqAsync(RabbitMqSettings settings, CancellationToken cancellationToken);
    }
}
