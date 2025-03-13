using AuditPilot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Repositories.Interfaces
{
    public interface IClientProjectRepository
    {
        Task AddAsync(ClientProject clientProject);
        Task<ClientProject> GetByIdAsync(Guid projectId);
        Task<IEnumerable<ClientProject>> GetAllAsync();
        Task UpdateAsync(ClientProject clientProject);
        Task DeleteAsync(Guid projectId);

        Task<List<ClientProject>> GetClientsProjectAsync(Guid clientId);
    }
}
