using Oniqys.Blazor.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Client.ViewModel
{
    public class UserViewModel : ContentBase
    {
        public string Id { get; init; }

        public string Name { get; set; }
    }
}
