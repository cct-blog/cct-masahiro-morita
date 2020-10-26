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
    }
}
