using AuditPilot.Data;
using AuditPilot.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Client client)
        {
           await _context.Clients.AddAsync(client);
            _context.SaveChanges();
        }

        public async Task<Client> GetByNameAsync(string name)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }


        public async Task<Client> GetByIdAsync(Guid id)
        {
            return await _context.Clients.FindAsync(id);
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _context.Clients.Where(x=>x.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Client>> GetAllAsync(string search, int page, int pageSize)
        {
            var query = _context.Clients.Where(x => x.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Name.Contains(search) || x.Email.Contains(search) || x.Phone.Contains(search));
            }

            return await query
                .OrderBy(x => x.Name) // Optional: Add sorting
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string search)
        {
            var query = _context.Clients.Where(x => x.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Name.Contains(search) || x.Email.Contains(search) || x.Phone.Contains(search));
            }

            return await query.CountAsync();
        }

        public async Task DeleteAsync(Client client)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ClientProject>> GetClientsProjectAsync(Guid clientId)
        {
            return await _context.ClientProjects
                .Where(cp => cp.ClientId == clientId)
                .ToListAsync();
        }

        

    }
}
