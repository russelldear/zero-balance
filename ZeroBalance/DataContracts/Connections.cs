using System;
using System.Collections.Generic;

namespace ZeroBalance.DataContracts
{
    public class Connections : List<Connection> { }

    public class Connection
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public string TenantType { get; set; }
    }
}