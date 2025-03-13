using AuditPilot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Repositories.Interfaces
{
    public interface IClientRepository
    {
        Task AddAsync(Client client);
        Task<Client> GetByIdAsync(Guid id);
        Task<IEnumerable<Client>> GetAllAsync();
        Task DeleteAsync(Client client);
    }
}
