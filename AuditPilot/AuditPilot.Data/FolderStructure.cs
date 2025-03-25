using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Data
{
    public class FolderStructure : EntityBase
    {
        [Required]
        [StringLength(255)]
        public string FolderName { get; set; }

        [StringLength(255)]
        public string ParentFolderId { get; set; } // Nullable for root folders

        [Required]
        [StringLength(255)]
        public string GoogleDriveFolderId { get; set; }
    }
}
