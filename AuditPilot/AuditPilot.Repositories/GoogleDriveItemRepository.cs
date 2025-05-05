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
    public class GoogleDriveItemRepository : IGoogleDriveItemRepository
    {
        private readonly ApplicationDbContext _context;

        public GoogleDriveItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> AddAsync(GoogleDriveItem item)
        {
            item.CreatedOn = DateTime.UtcNow; // Use UTC for consistency
            item.ModifiedOn = DateTime.UtcNow;
            await _context.GoogleDriveItems.AddAsync(item);
            await _context.SaveChangesAsync();
            return item.Id;
        }

        public async Task<List<GoogleDriveItem>> GetByGoogleIdsAsync(List<string> googleIds)
        {
            if (googleIds == null || !googleIds.Any())
                return new List<GoogleDriveItem>();

            return await _context.GoogleDriveItems
                .Where(item => googleIds.Contains(item.GoogleId))
                .ToListAsync();
        }

        public async Task<GoogleDriveItem> GetByGoogleIdAsync(string googleId)
        {
            if (string.IsNullOrEmpty(googleId))
                return null;

            return await _context.GoogleDriveItems
                .FirstOrDefaultAsync(item => item.GoogleId == googleId);
        }

        public async Task UpdateAsync(GoogleDriveItem item)
        {
            _context.GoogleDriveItems.Update(item);
            await _context.SaveChangesAsync();
        }

    }
}
