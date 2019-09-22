using System;
using System.Collections.Generic;

namespace ZeroBalance.DataContracts
{
    public class Organisations : List<Organisation> {}

    public class Organisation
    {
        public string Name { get; set; }
    }
}
