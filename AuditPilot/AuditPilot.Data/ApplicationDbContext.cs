using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

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
        public DbSet<UserProjectPermission> UserProjectPermissions { get; set; }

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
                new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "AuditManager", NormalizedName = "AUDITMANAGER" }, // Space hata diya
                new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "TaxManager", NormalizedName = "TAXMANAGER" },   // Space hata diya
                new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "User", NormalizedName = "USER" }
            };

            builder.Entity<IdentityRole>().HasData(roles);

            // UserProjectPermission configuration
            builder.Entity<UserProjectPermission>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.UserId).IsRequired();
                entity.Property(p => p.ProjectId).IsRequired();
                entity.Property(p => p.HasAccess).IsRequired();
                entity.Property(p => p.AssignedOn).IsRequired();

                // Relationships
                entity.HasOne(p => p.User)
                      .WithMany()
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Project)
                      .WithMany()
                      .HasForeignKey(p => p.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}