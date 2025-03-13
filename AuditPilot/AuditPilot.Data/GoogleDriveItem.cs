using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Data
{
    public class GoogleDriveItem : EntityBase
    {
        public bool IsFolder {  get; set; }

        public string FileName { get; set; }

        public string? GoogleId { get; set; }
    }
}
