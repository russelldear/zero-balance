using System.Collections.Generic;
using System.Net.Http;

namespace ZeroBalance.Services
{
    public interface IHttpClient
    {
        HttpResponseMessage Get(string url, Dictionary<string, string> headers = null);
    }
}