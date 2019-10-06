using System.Collections.Generic;

namespace ZeroBalance.DataContracts
{
    public class Invoices : List<Invoice> { }

	public class Invoice
	{
		public float AmountDue { get; set; }

        public string CurrencyCode { get; set; }
    }
}
