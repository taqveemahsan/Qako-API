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

        public async Task<Client> GetByIdAsync(Guid id)
        {
            return await _context.Clients.FindAsync(id);
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _context.Clients.Where(x=>x.IsActive).ToListAsync();
        }

        public async Task DeleteAsync(Client client)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }


        // Implement other CRUD operations as needed
    }
}
