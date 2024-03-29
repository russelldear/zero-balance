﻿using System;
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

        private Currencies _currencies;

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
                _currencies = GetCurrencies(organisation);

                combinedResponse += $"For organisation {organisation.Name}, {GetInvoicesBalance(organisation)}; and {GetBillsBalance(organisation)}. ";
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
            var invoicesByCurrency = GetInvoices(organisation, InvoiceType.Invoice);

            if (invoicesByCurrency.Count() > 0)
            {
                if (invoicesByCurrency.Count() == 1)
                {
                    return $"you have {invoicesByCurrency.Single().Value.Count()} outstanding invoices, totalling {invoicesByCurrency.Single().Value.Sum(i => i.AmountDue)} {_currencies.First().Description}s";
                }
                else
                {
                    var result = $"you have {invoicesByCurrency.Sum(i => i.Value.Count())} outstanding invoices, comprising of ";

                    var currencyCount = invoicesByCurrency.Keys.Count();

                    var iteration = 1;

                    foreach (var currency in invoicesByCurrency.Keys)
                    {
                        var invoices = invoicesByCurrency[currency];

                        result += $"{invoices.Sum(i => i.AmountDue)} {currency}s,";

                        if (iteration == currencyCount - 1)
                        {
                            result += " and ";
                        }

                        iteration++;
                    }

                    return result;
                }
            }

            return "you have no outstanding invoices";
        }

        private string GetBillsBalance(Organisation organisation)
        {
            var billsByCurrency = GetInvoices(organisation, InvoiceType.Bill);

            if (billsByCurrency.Count() > 0)
            {
                if (billsByCurrency.Count() == 1)
                {
                    return $"you have {billsByCurrency.Single().Value.Count()} bills to pay, totalling {billsByCurrency.Single().Value.Sum(i => i.AmountDue)} {_currencies.First().Description}s";
                }
                else
                {
                    var result = $"you have {billsByCurrency.Sum(b => b.Value.Count())} bills to pay, comprising of ";

                    var currencyCount = billsByCurrency.Keys.Count();

                    var iteration = 1;

                    foreach (var currency in billsByCurrency.Keys)
                    {
                        var bills = billsByCurrency[currency];

                        result += $"{bills.Sum(i => i.AmountDue)} {currency}s,";

                        if (iteration == currencyCount - 1)
                        {
                            result += " and ";
                        }

                        iteration++;
                    }

                    return result;
                }
            }

            return "you have no bills to pay";
        }

        private Dictionary<string, IEnumerable<Invoice>> GetInvoices(Organisation organisation, string type)
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

                    var invoices = JsonConvert.DeserializeObject<XeroApiResponse>(invoicesResponseString).Invoices;

                    return GetInvoicesByCurrency(organisation, invoices);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorisedException();
                }
            }

            return null;
        }

        private Dictionary<string, IEnumerable<Invoice>> GetInvoicesByCurrency(Organisation organisation, Invoices invoices)
        {
            var result = new Dictionary<string, IEnumerable<Invoice>>();

            foreach (var currency in _currencies)
            {
                if (invoices.Any(i => i.CurrencyCode == currency.Code))
                {
                    result.Add(currency.Description, invoices.Where(i => i.CurrencyCode == currency.Code).ToList());
                }
            }

            return result;
        }

        private Currencies GetCurrencies(Organisation organisation)
        {
            HttpResponseMessage response = null;

            var currencyUrl = $"{Settings.XeroBaseUrl}{ApiAccounting}{Endpoints.Currencies}";

            try
            {
                response = _httpClient.Get(currencyUrl, new Dictionary<string, string> { { Headers.XeroTenantId, $"{organisation.OrganisationID}" } });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Currencies request failed: " + ex.Message);
            }

            if (response != null)
            {
                Console.WriteLine("Xero currencies request status: " + response.StatusCode);

                if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                {
                    var currenciesResponseString = response.Content.ReadAsStringAsync().Result;

                    Console.WriteLine(Regex.Replace(currenciesResponseString, @"\r\n?|\n", " "));

                    return JsonConvert.DeserializeObject<XeroApiResponse>(currenciesResponseString).Currencies;
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