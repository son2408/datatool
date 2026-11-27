using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DataTool.App.Models;
using DataTool.App.Services;
using DataTool.UI.Controls;

namespace DataTool.App.ViewModels
{
    /// <summary>Form sửa một bộ cấu hình (làm việc trên bản sao, chỉ ghi khi bấm Lưu).</summary>
    public class ProfileEditorViewModel : ObservableObject
    {
        private readonly EnvironmentProfile _p;

        public ProfileEditorViewModel(EnvironmentProfile profile, IConnectionTester tester)
        {
            _p = profile.Clone();
            DatabaseTest = new ConnectionTestViewModel(ct => tester.TestDatabaseAsync(_p.Database.Clone(), ct));
            SolrTest = new ConnectionTestViewModel(ct => tester.TestSolrAsync(_p.Solr.Clone(), ct));
            RabbitMqTest = new ConnectionTestViewModel(ct => tester.TestRabbitMqAsync(_p.RabbitMq.Clone(), ct));
        }

        public Guid Id => _p.Id;

        public ConnectionTestViewModel DatabaseTest { get; }
        public ConnectionTestViewModel SolrTest { get; }
        public ConnectionTestViewModel RabbitMqTest { get; }

        // ----- Chung -----
        public string Name
        {
            get => _p.Name;
            set => SetProperty(_p.Name, value ?? string.Empty, _p, (m, v) => m.Name = v);
        }

        public EnvironmentLevel Level
        {
            get => _p.Level;
            set
            {
                if (SetProperty(_p.Level, value, _p, (m, v) => m.Level = v))
                    OnPropertyChanged(nameof(IsProduction));
            }
        }

        public bool IsProduction => Level == EnvironmentLevel.Production;

        // ----- CSDL Oracle -----
        public string DbHost
        {
            get => _p.Database.Host;
            set { if (SetProperty(_p.Database.Host, value ?? string.Empty, _p.Database, (m, v) => m.Host = v)) DatabaseTest.Reset(); }
        }

        public int DbPort
        {
            get => _p.Database.Port;
            set { if (SetProperty(_p.Database.Port, value, _p.Database, (m, v) => m.Port = v)) DatabaseTest.Reset(); }
        }

        public string DbServiceName
        {
            get => _p.Database.ServiceName;
            set { if (SetProperty(_p.Database.ServiceName, value ?? string.Empty, _p.Database, (m, v) => m.ServiceName = v)) DatabaseTest.Reset(); }
        }

        public string DbUserName
        {
            get => _p.Database.UserName;
            set { if (SetProperty(_p.Database.UserName, value ?? string.Empty, _p.Database, (m, v) => m.UserName = v)) DatabaseTest.Reset(); }
        }

        public string DbPassword
        {
            get => _p.Database.Password;
            set { if (SetProperty(_p.Database.Password, value ?? string.Empty, _p.Database, (m, v) => m.Password = v)) DatabaseTest.Reset(); }
        }

        // ----- SOLR -----
        public string SolrUrl
        {
            get => _p.Solr.Url;
            set { if (SetProperty(_p.Solr.Url, value ?? string.Empty, _p.Solr, (m, v) => m.Url = v)) SolrTest.Reset(); }
        }

        public string SolrCore
        {
            get => _p.Solr.Core;
            set { if (SetProperty(_p.Solr.Core, value ?? string.Empty, _p.Solr, (m, v) => m.Core = v)) SolrTest.Reset(); }
        }

        public string SolrUserName
        {
            get => _p.Solr.UserName;
            set { if (SetProperty(_p.Solr.UserName, value ?? string.Empty, _p.Solr, (m, v) => m.UserName = v)) SolrTest.Reset(); }
        }

        public string SolrPassword
        {
            get => _p.Solr.Password;
            set { if (SetProperty(_p.Solr.Password, value ?? string.Empty, _p.Solr, (m, v) => m.Password = v)) SolrTest.Reset(); }
        }

        // ----- RabbitMQ -----
        public string MqHost
        {
            get => _p.RabbitMq.Host;
            set { if (SetProperty(_p.RabbitMq.Host, value ?? string.Empty, _p.RabbitMq, (m, v) => m.Host = v)) RabbitMqTest.Reset(); }
        }

        public int MqPort
        {
            get => _p.RabbitMq.Port;
            set { if (SetProperty(_p.RabbitMq.Port, value, _p.RabbitMq, (m, v) => m.Port = v)) RabbitMqTest.Reset(); }
        }

        public string MqVirtualHost
        {
            get => _p.RabbitMq.VirtualHost;
            set { if (SetProperty(_p.RabbitMq.VirtualHost, value ?? string.Empty, _p.RabbitMq, (m, v) => m.VirtualHost = v)) RabbitMqTest.Reset(); }
        }

        public string MqUserName
        {
            get => _p.RabbitMq.UserName;
            set { if (SetProperty(_p.RabbitMq.UserName, value ?? string.Empty, _p.RabbitMq, (m, v) => m.UserName = v)) RabbitMqTest.Reset(); }
        }

        public string MqPassword
        {
            get => _p.RabbitMq.Password;
            set { if (SetProperty(_p.RabbitMq.Password, value ?? string.Empty, _p.RabbitMq, (m, v) => m.Password = v)) RabbitMqTest.Reset(); }
        }

        public EnvironmentProfile ToModel() => _p.Clone();

        /// <summary>Tạo bản sao với Id mới (nút "Nhân bản").</summary>
        public EnvironmentProfile CreateCopy(string newName)
        {
            var copy = _p.Clone();
            copy.Id = Guid.NewGuid();
            copy.Name = newName;
            return copy;
        }
    }
}
