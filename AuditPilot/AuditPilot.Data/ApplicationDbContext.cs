using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AuditPilot.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Client> Clients { get; set; }
        public DbSet<GoogleDriveItem> GoogleDriveItems { get; set; }
        public DbSet<ClientProject> ClientProjects { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var primaryKey = entityType.FindPrimaryKey();
                if (primaryKey == null)
                {
                    builder.Entity(entityType.ClrType).HasKey("Id");
                    builder.Entity(entityType.ClrType)
                        .Property("Id")
                        .HasDefaultValueSql("NEWID()");
                }
            }

            var roles = new List<IdentityRole>
        {
            new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Partner", NormalizedName = "PARTNER" },
            new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Audit Manager", NormalizedName = "AUDITMANAGER" },
            new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Tax Manager", NormalizedName = "TAXMANAGER" },
            new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "User", NormalizedName = "USER" }
        };

            builder.Entity<IdentityRole>().HasData(roles);

        }
    }
}
