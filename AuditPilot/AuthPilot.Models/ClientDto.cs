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
}
