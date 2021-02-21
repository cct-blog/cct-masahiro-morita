using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Client.Shared.Components
{
    public class SelectableContext<TItem>
    {
        public bool IsSelected { get; set; }

        public bool IsEnabled { get; set; }

        public TItem Value { get; set; }
    }
}
