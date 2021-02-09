using blazorTest.Server.Data;
using blazorTest.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserInformation>> ReadUsersInfomation()
            => await _context.Users
            .Select(_user => new UserInformation()
            {
                HandleName = _user.HandleName,
                Email = _user.Email
            })
            .ToArrayAsync();
    }
}