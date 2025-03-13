using AuditPilot.Data;
using AuditPilot.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Repositories
{
    public class GoogleDriveItemRepository : IGoogleDriveItemRepository
    {
        private readonly ApplicationDbContext _context;

        public GoogleDriveItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> AddAsync(GoogleDriveItem item)
        {
            item.CreatedOn = DateTime.Now;
            await _context.GoogleDriveItems.AddAsync(item);
            await _context.SaveChangesAsync();
            return item.Id;
        }

        // Implement other CRUD operations as needed
    }
}
