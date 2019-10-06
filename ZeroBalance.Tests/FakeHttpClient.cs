using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using ZeroBalance.Services;

namespace ZeroBalance.Tests
{
    public class FakeHttpClient : IHttpClient
    {
        private readonly HttpStatusCode _status;
        private readonly bool _multicurrency;

        public FakeHttpClient(HttpStatusCode status, bool multicurrency)
        {
            _status = status;
            _multicurrency = multicurrency;
        }

        public HttpResponseMessage Get(string url, Dictionary<string, string> headers)
        {
            if (_status == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            var result = new HttpResponseMessage
            {
                StatusCode = _status
            };

            if (url.Contains("connections"))
            {
                result.Content = new StringContent("[ { \"id\": \"57d90938-314e-4296-9a62-5fb3dd79b2b9\", \"tenantId\":\"57d90938-314e-4296-9a62-5fb3dd79b2b9\", \"tenantType\":\"ORGANISATION\" } ]");
            }

            if (url.Contains("organisations"))
            {
                result.Content = new StringContent("{ \"Organisations\":[ { \"OrganisationID\": \"57d90938-314e-4296-9a62-5fb3dd79b2b9\", \"Name\": \"Something\" } ] }");
            }

            if (url.Contains("currencies"))
            {
                result.Content = new StringContent("{ \"Currencies\":[ { \"Code\": \"AUD\", \"Description\": \"Australian dollar\" }, { \"Code\": \"EUR\", \"Description\": \"Euro\" } ] }");
            }

            if (_multicurrency)
            {
                if (url.Contains("invoices"))
                {
                    result.Content = new StringContent("{ \"Invoices\":[ { \"AmountDue\": \"123.45\", \"CurrencyCode\": \"AUD\" }, { \"AmountDue\": \"123.45\", \"CurrencyCode\": \"EUR\" } ] }");
                }
            }
            else
            {
                if (url.Contains("invoices"))
                {
                    result.Content = new StringContent("{ \"Invoices\":[ { \"AmountDue\": \"123.45\", \"CurrencyCode\": \"AUD\" } ] }");
                }
            }

            return result;
        }
    }
}
