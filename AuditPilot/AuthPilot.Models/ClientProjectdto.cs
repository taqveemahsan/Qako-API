using AuthPilot.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthPilot.Models
{
    public class ClientProjectdto
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; }
        public Guid ClientId { get; set; }
        public string GoogleDriveFolderId { get; set; }
        public ProjectType ProjectType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
