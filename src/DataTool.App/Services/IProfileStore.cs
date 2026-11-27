using System.Collections.Generic;
using DataTool.App.Models;

namespace DataTool.App.Services
{
    public interface IProfileStore
    {
        IList<EnvironmentProfile> Load();
        void Save(IEnumerable<EnvironmentProfile> profiles);
    }
}
