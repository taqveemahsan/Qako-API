using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Data
{
    public class UserProjectPermission : EntityBase
    {
        //public int Id { get; set; }
        public string UserId { get; set; }
        public Guid ProjectId { get; set; }
        public bool HasAccess { get; set; }
        public DateTime AssignedOn { get; set; }
        public DateTime ExpiredOn { get; set; }

        public ApplicationUser User { get; set; }
        public ClientProject Project { get; set; }
    }
}
