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
    public class FolderStructureRepository : IFolderStructureRepository
    {
        private readonly ApplicationDbContext _context;

        public FolderStructureRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetFolderIdAsync(string folderName, string parentFolderId)
        {
            var folder = await _context.FolderStructures
                .Where(f => f.FolderName == folderName && f.ParentFolderId == parentFolderId && f.IsActive)
                .Select(f => f.GoogleDriveFolderId)
                .FirstOrDefaultAsync();

            return folder;
        }

        public async Task AddFolderAsync(string folderName, string parentFolderId, string googleDriveFolderId)
        {
            var folder = new FolderStructure
            {
                FolderName = folderName,
                ParentFolderId = parentFolderId,
                GoogleDriveFolderId = googleDriveFolderId,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };

            _context.FolderStructures.Add(folder);
            await _context.SaveChangesAsync();
        }
    }
}
