using blazorTest.Server.Data;
using blazorTest.Server.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace blazorTest.ServerTests.ContextBase
{
    public class PostServiceBase : IDisposable
    {
        private static readonly object _lock = new object();
        private static bool _databaseInitialized;

        public DbConnection Connection { get; }

        public PostServiceBase()
        {
            Connection = new SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Database=study;Trusted_Connection=True;");

            Seed();

            Connection.Open();
        }

        public ApplicationDbContext CreateContext(DbTransaction transaction = null)
        {
            var context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(Connection).Options,
                Options.Create(new OperationalStoreOptions()));

            if (transaction != null)
            {
                context.Database.UseTransaction(transaction);
            }

            return context;
        }

        private void Seed()
        {
            lock (_lock)
            {
                if (!_databaseInitialized)
                {
                    using (var context = CreateContext())
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();

                        Enumerable.Range(1, 4)
                            .Select(index =>
                            {
                                var applicationUser = new ApplicationUser();
                                applicationUser.Id = "a@b" + index.ToString();
                                applicationUser.PasswordHash = "test@test.com";
                                applicationUser.SecurityStamp = Guid.NewGuid().ToString();
                                context.Users.Add(applicationUser);

                                var room = new Room();
                                room.Name = "room" + index.ToString();
                                context.Rooms.Add(room);

                                return "";
                            }).ToList();
                        context.SaveChanges();

                        var rooms = context.Rooms.ToList();

                        RegisterUserInfoInRoom(context, "a@b2", rooms[0].Id);
                        RegisterUserInfoInRoom(context, "a@b3", rooms[0].Id);
                        RegisterUserInfoInRoom(context, "a@b3", rooms[1].Id);
                        RegisterUserInfoInRoom(context, "a@b4", rooms[0].Id);
                        RegisterUserInfoInRoom(context, "a@b4", rooms[1].Id);
                        RegisterUserInfoInRoom(context, "a@b4", rooms[2].Id);

                        Enumerable.Range(1, 100)
                            .Select(index =>
                            {
                                var post = new Post();
                                post.ApplicationUserId = "a@b2";
                                post.RoomId = rooms.Where(room => room.Name == "room1").Single().Id;
                                post.Text = "data" + index.ToString();
                                context.Posts.Add(post);
                                return post;
                            }).ToList();
                        context.SaveChanges();
                    }

                    _databaseInitialized = true;
                }
            }
        }

        private void RegisterUserInfoInRoom(ApplicationDbContext context, string userId, Guid roomId)
        {
            var userInfoInRoom = new UserInfoInRoom();
            userInfoInRoom.ApplicationUserId = userId;
            userInfoInRoom.RoomId = roomId;
            context.UserInfoInRooms.Add(userInfoInRoom);
        }

        public void Dispose() => Connection.Dispose();
    }
}