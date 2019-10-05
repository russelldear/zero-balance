using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using ZeroBalance.DataContracts;
using static ZeroBalance.Constants;

namespace ZeroBalance.Services
{
    public class XeroService : IXeroService
    {
        private readonly IHttpClient _httpClient;
        private readonly Settings Settings = new Settings();

        public XeroService(string accessToken) : this(new XeroClient(accessToken))
        {
        }

        public XeroService(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string GetConnections()
        {
            var organisations = GetOrganisations();

            if (organisations.Count() > 0)
            {
                var responseBuilder = new StringBuilder("You have connected the following organisations to Zero Balance: ");

                foreach (var organisation in organisations)
                {
                    responseBuilder.Append($"{organisation.Name}, ");
                }

                return responseBuilder.ToString();
            }

            return "Something went wrong. Please try again in a few minutes.";
        }

        public string GetBalances()
        {
            var organisations = GetOrganisations();

            var combinedResponse = string.Empty;

            foreach (var organisation in organisations)
            {
                combinedResponse = $"For organisation {organisation.Name}, {GetInvoicesBalance(organisation)} and {GetBillsBalance(organisation)}";
            }

            return combinedResponse;
        }

        private Organisations GetOrganisations()
        {
            HttpResponseMessage response = null;

            var connectionsUrl = $"{Settings.XeroBaseUrl}{Endpoints.Connections}";

            try
            {
                response = _httpClient.Get(connectionsUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connections request failed: " + ex.Message);
            }

            if (response != null)
            {
                Console.WriteLine("Xero connections request status: " + response.StatusCode);

                if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                {
                    var connectionsResponseString = response.Content.ReadAsStringAsync().Result;

                    Console.WriteLine(connectionsResponseString);

                    var connections = JsonConvert.DeserializeObject<Connections>(connectionsResponseString);

                    var organisations = new Organisations();

                    foreach (var connection in connections)
                    {
                        var organisationsUrl = $"{Settings.XeroBaseUrl}{ApiAccounting}{Endpoints.Organisations}";

                        response = _httpClient.Get(organisationsUrl, new Dictionary<string, string> { { Headers.XeroTenantId, $"{connection.TenantId}" } });

                        var apiResponseString = response.Content.ReadAsStringAsync().Result;

                        var apiResponse = JsonConvert.DeserializeObject<XeroApiResponse>(apiResponseString);

                        organisations.AddRange(apiResponse.Organisations);
                    }

                    return organisations;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorisedException();
                }
            }

            return null;
        }

        private string GetInvoicesBalance(Organisation organisation)
        {
            var invoices = GetInvoices(organisation, InvoiceType.Invoice);

            if (invoices.Count() > 0)
            {
                return $"you have {invoices.Count()} outstanding invoices totalling ${invoices.Sum(i => i.AmountDue)}";
            }

            return "you have no outstanding invoices";
        }

        private string GetBillsBalance(Organisation organisation)
        {
            var bills = GetInvoices(organisation, InvoiceType.Bill);

            if (bills.Count() > 0)
            {
                return $"you have {bills.Count()} bills to pay totalling ${bills.Sum(i => i.AmountDue)}";
            }

            return "you have no bills to pay";
        }

        private Invoices GetInvoices(Organisation organisation, string type)
        {
            HttpResponseMessage response = null;

            var whereClause = $"where={HttpUtility.UrlEncode($"Type==\"{type}\"&&Status==\"{InvoiceStatus.Authorised}\"")}";

            var invoiceUrl = $"{Settings.XeroBaseUrl}{ApiAccounting}{Endpoints.Invoices}?{whereClause}";

            Console.WriteLine(invoiceUrl);

            try
            {
                response = _httpClient.Get(invoiceUrl, new Dictionary<string, string> { { Headers.XeroTenantId, $"{organisation.OrganisationID}" } });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invoices request failed: " + ex.Message);
            }

            if (response != null)
            {
                Console.WriteLine("Xero invoices request status: " + response.StatusCode);

                if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                {
                    var invoicesResponseString = response.Content.ReadAsStringAsync().Result;

                    Console.WriteLine(Regex.Replace(invoicesResponseString, @"\r\n?|\n", " "));

                    return JsonConvert.DeserializeObject<XeroApiResponse>(invoicesResponseString).Invoices;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorisedException();
                }
            }

            return null;
        }
    }
}