using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using ZeroBalance.DataContracts;

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
            HttpResponseMessage response = null;

            var connectionsUrl = $"{Settings.XeroBaseUrl}/connections";

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

                    var responseBuilder = new StringBuilder("You have connected the following organisations to Zero Balance: ");

                    foreach (var connection in connections)
                    {
                        var organisationsUrl = $"{Settings.XeroBaseUrl}/api.xro/2.0/organisations";

                        response = _httpClient.Get(organisationsUrl, new Dictionary<string, string> { { "xero-tenant-id", $"{connection.TenantId}" } });

                        var apiResponseString = response.Content.ReadAsStringAsync().Result;

                        var apiResponse = JsonConvert.DeserializeObject<XeroApiResponse>(apiResponseString);

                        responseBuilder.Append($"{apiResponse.Organisations.Single().Name}, ");
                    }

                    return responseBuilder.ToString();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorisedException();
                }
            }

            return "Something went wrong. Please try again in a few minutes.";
        }
    }
}