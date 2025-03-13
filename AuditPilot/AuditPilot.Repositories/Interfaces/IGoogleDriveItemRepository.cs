using AuditPilot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Repositories.Interfaces
{
    public interface IGoogleDriveItemRepository
    {
        Task<Guid> AddAsync(GoogleDriveItem item);
    }
}
