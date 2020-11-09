using blazorTest.Server.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public DbSet<Thread> Threads { get; set; }
        public DbSet<UserInfoInRoom> UserInfoInRooms { get; set; }

        public override int SaveChanges()
        {
            SetProperties();
            return base.SaveChanges();
        }

        public void SetProperties()
        {
            foreach (var entityEntry in ChangeTracker.Entries()
                .Where(entry => entry.State == EntityState.Added))
            {
                var createEntity = entityEntry.Entity as ICreateAndUpdateDate;
                if (createEntity is null)
                {
                    createEntity.CreateDate = DateTime.Now;
                    createEntity.UpdateDate = DateTime.Now;
                }
            }

            foreach (var entityEntry in ChangeTracker.Entries()
                .Where(entry => entry.State == EntityState.Modified))
            {
                var updateEntity = entityEntry.Entity as ICreateAndUpdateDate;
                if (updateEntity is null) updateEntity.UpdateDate = DateTime.Now;
            }
        }
    }
}