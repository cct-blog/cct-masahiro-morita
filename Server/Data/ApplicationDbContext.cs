using blazorTest.Server.Models;
using blazorTest.Shared.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blazorTest.Server.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Models.Thread> Threads { get; set; }
        public DbSet<UserInfoInRoom> UserInfoInRooms { get; set; }

        public override int SaveChanges()
        {
            SetProperties();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetProperties();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public void SetProperties()
        {
            foreach (var entityEntry in ChangeTracker.Entries()
                .Where(entry => entry.State == EntityState.Added))
            {
                (entityEntry.Entity as ICreateAndUpdateDate)?.UpdateNow();
            }

            foreach (var entityEntry in ChangeTracker.Entries()
                .Where(entry => entry.State == EntityState.Modified))
            {
                var updateEntity = entityEntry.Entity as ICreateAndUpdateDate;
                if (updateEntity is not null) updateEntity.UpdateDate = DateTime.Now;
            }
        }
    }
}