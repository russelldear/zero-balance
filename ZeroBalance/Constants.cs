namespace ZeroBalance
{
    public static class Constants
    {
        public const string LaunchRequest = "LaunchRequest";
        public const string IntentRequest = "IntentRequest";

        public const string ConnectionsIntent = "ConnectionsIntent";
        public const string BalancesIntent = "BalancesIntent";
        public const string VersionIntent = "VersionIntent";
        public const string HelpIntent = "AMAZON.HelpIntent";
        public const string StopIntent = "AMAZON.StopIntent";
        public const string CancelIntent = "AMAZON.CancelIntent";

        public const string DefaultLaunchRequestText = "Zero Balance gives you balances from your bank accounts in Xero. Would you like to hear details of your balances?";
        public const string HelpRequestText = "Zero Balance gives you balances from your bank accounts in Xero. Would you like to hear details of your balances?";

        public const string UnauthorisedResponse = "You haven't authorised access to your Xero account. Use the Alexa app to link your account and access your Xero information.";

        public const string BaseUrlKey = "XeroBaseUrl";

        public static class Headers
        {
            public const string XeroTenantId = "xero-tenant-id";
        }

        public static class TenantTypes
        {
            public const string Organisation = "ORGANISATION";
        }

        public const string ApiAccounting = "/api.xro/2.0";

        public static class Endpoints
        {
            public const string Connections = "/connections";
            public const string Organisations = "/organisations";
            public const string Invoices = "/invoices";
        }

        public static class InvoiceStatus
        {
            public const string Authorised = "AUTHORISED";
        }

        public static class InvoiceType
        {
            public const string Invoice = "ACCREC";
            public const string Bill = "ACCPAY";
        }
    }
}
