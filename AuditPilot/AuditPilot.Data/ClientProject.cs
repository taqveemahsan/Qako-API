using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Data
{
    public class ClientProject : EntityBase
    {
        public string ProjectName { get; set; }

        // Foreign key referencing the Client entity
        public Guid ClientId { get; set; }
        [ForeignKey("ClientId")]
        public Client Client { get; set; }

        // Enum to represent ProjectType
        public int ProjectType { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string GoogleDriveFolderId { get; set; }
    }

    // Enum to define ProjectType values

}
