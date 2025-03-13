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
    public class ClientProjectRepository : IClientProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ClientProject clientProject)
        {
            await _context.ClientProjects.AddAsync(clientProject);
            await _context.SaveChangesAsync();
        }

        public async Task<ClientProject> GetByIdAsync(Guid projectId)
        {
            return await _context.ClientProjects
                .Include(cp => cp.Client) // Include related Client data
                .FirstOrDefaultAsync(cp => cp.Id == projectId);
        }

        public async Task<IEnumerable<ClientProject>> GetAllAsync()
        {
            return await _context.ClientProjects
                .Include(cp => cp.Client) // Include related Client data
                .ToListAsync();
        }

        public async Task<List<ClientProject>> GetClientsProjectAsync(Guid clientId)
        {
            return await _context.ClientProjects.Where(x=>x.ClientId == clientId)
                .Include(cp => cp.Client) // Include related Client data
                .ToListAsync();
        }

        public async Task UpdateAsync(ClientProject clientProject)
        {
            _context.ClientProjects.Update(clientProject);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid projectId)
        {
            var clientProject = await GetByIdAsync(projectId);
            if (clientProject != null)
            {
                _context.ClientProjects.Remove(clientProject);
                await _context.SaveChangesAsync();
            }
        }
    }
}
