using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Data.ViewModels
{
    public class FolderStructureDto
    {
        public string FolderName { get; set; }
        public string ParentFolderId { get; set; }
        public string GoogleDriveFolderId { get; set; }
        public string ClientName { get; set; } // Client ka naam
        public string ClientType { get; set; } // Client type (PrivateLabel ya PublicLabel)
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
    }
}
