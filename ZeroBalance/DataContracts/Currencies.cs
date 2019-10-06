using System.Collections.Generic;

namespace ZeroBalance.DataContracts
{
    public class Currencies : List<Currency> { }

    public class Currency
    {
        public string Code { get; set; }

        public string Description { get; set; }
    }
}
