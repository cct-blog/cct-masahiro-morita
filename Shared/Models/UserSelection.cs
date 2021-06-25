using System;
using System.Collections.Generic;

namespace ChatApp.Shared.Models
{
    public class UserSelection
    {
        public List<User> Users { get; set; }

        public List<Guid> SeletedUserIds { get; set; }
    }
}
