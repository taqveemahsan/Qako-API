using AuditPilot.Data;
using AuditPilot.Data.ViewModels;
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
                ParentFolderId = parentFolderId == null ? "": parentFolderId,
                GoogleDriveFolderId = googleDriveFolderId,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };

            _context.FolderStructures.Add(folder);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FolderStructureDto>> GetFolderStructureListAsync(string search = "", int page = 1, int pageSize = 10)
        {
            var query = _context.FolderStructures
                .Where(f => f.IsActive); // Only active folders

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(f => f.FolderName.ToLower().Contains(search));
            }

            // Join with Clients table to get ClientType
            var result = await query
                .GroupJoin(
                    _context.Clients,
                    folder => folder.FolderName,
                    client => client.Name,
                    (folder, clients) => new { folder, clients }
                )
                .SelectMany(
                    x => x.clients.DefaultIfEmpty(),
                    (x, client) => new FolderStructureDto
                    {
                        FolderName = x.folder.FolderName,
                        ParentFolderId = x.folder.ParentFolderId,
                        GoogleDriveFolderId = x.folder.GoogleDriveFolderId,
                        ClientName = client != null ? client.Name : "",
                        //ClientType = client != null ? (client.CompanyType == (int) ? "PrivateLabel" : "PublicLabel") : null,
                        CreatedOn = x.folder.CreatedOn,
                        IsActive = x.folder.IsActive
                    }
                )
                .OrderBy(f => f.FolderName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return result;
        }

        public async Task<int> GetFolderStructureCountAsync(string search = "")
        {
            var query = _context.FolderStructures
                .Where(f => f.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(f => f.FolderName.ToLower().Contains(search));
            }

            return await query.CountAsync();
        }

        public async Task UpdateFolderNameByGoogleDriveIdAsync(string googleDriveFolderId, string newFolderName)
        {
            var folder = await _context.FolderStructures.FirstOrDefaultAsync(f => f.GoogleDriveFolderId == googleDriveFolderId && f.IsActive);
            if (folder != null)
            {
                folder.FolderName = newFolderName;
                await _context.SaveChangesAsync();
            }
        }
    }
}
