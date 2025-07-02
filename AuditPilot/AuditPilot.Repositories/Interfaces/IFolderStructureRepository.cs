using AuditPilot.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Repositories.Interfaces
{
    public interface IFolderStructureRepository
    {
        Task<string> GetFolderIdAsync(string folderName, string parentFolderId);
        Task AddFolderAsync(string folderName, string parentFolderId, string googleDriveFolderId);
        Task<List<FolderStructureDto>> GetFolderStructureListAsync(string search = "", int page = 1, int pageSize = 10);
        Task<int> GetFolderStructureCountAsync(string search = "");
        Task UpdateFolderNameByGoogleDriveIdAsync(string googleDriveFolderId, string newFolderName);
    }
}
