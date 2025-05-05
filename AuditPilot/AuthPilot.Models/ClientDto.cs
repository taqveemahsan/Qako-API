using AuthPilot.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthPilot.Models
{
    public class ClientDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CompanyType CompanyType { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class ClientDtoViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CompanyType CompanyType { get; set; }
        public string Email { get; set; }
        public string FolderSize { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedByUserName { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public bool IsActive { get; set; }
        public string GoogleDriveId { get; set; } = "";
    }
}
