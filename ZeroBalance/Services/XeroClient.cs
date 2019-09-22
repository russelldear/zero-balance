using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ZeroBalance.Services
{
    public class XeroClient : IHttpClient
    {
        readonly string _accessToken;

        public XeroClient(string accessToken)
        {
            _accessToken = accessToken;
        }

        public HttpResponseMessage Get(string url, Dictionary<string, string> headers = null)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    if (headers != null)
                    {
                        foreach (var headerName in headers.Keys)
                        {
                            if (client.DefaultRequestHeaders.TryGetValues(headerName, out _))
                            {
                                client.DefaultRequestHeaders.Remove(headerName);
                            }

                            client.DefaultRequestHeaders.Add(headerName, headers[headerName]);
                        }
                    }

                    return client.GetAsync(url).Result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Xero request failed: " + ex.Message);
                }
            }

            return null;
        }
    }
}
