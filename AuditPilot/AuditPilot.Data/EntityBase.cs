using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Data
{
    public abstract class EntityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }

        public Guid ModifiedBy { get; set; }

        public DateTime ModifiedOn { get; set; }
        public bool  IsActive { get; set; }
    }
}
