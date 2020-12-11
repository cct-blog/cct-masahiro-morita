using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Models
{
    internal interface ICreateAndUpdateDate
    {
        DateTime CreateDate { get; set; }
        DateTime UpdateDate { get; set; }

        public void UpdateNow() => CreateDate = UpdateDate = DateTime.Now;
    }
}