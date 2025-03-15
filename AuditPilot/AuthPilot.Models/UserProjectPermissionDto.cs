﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthPilot.Models
{
    public class UserProjectPermissionDto
    {
        public string UserId { get; set; }
        public Guid ProjectId { get; set; }
        public bool HasAccess { get; set; }
    }
}
