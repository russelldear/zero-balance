namespace ZeroBalance.DataContracts
{
    public class XeroApiResponse
    {
        public Organisations Organisations { get; set; }

        public Invoices Invoices { get; set; }

        public Currencies Currencies { get; set; }
    }
}
