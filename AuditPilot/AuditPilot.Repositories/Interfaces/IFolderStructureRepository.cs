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
    }
}
