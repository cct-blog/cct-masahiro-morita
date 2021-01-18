using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorTest.Shared.Models
{
    public record UserInformation
    {
        public string HandleName { get; init; }

        public string Email { get; init; }
    }
}