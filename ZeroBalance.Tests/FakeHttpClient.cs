﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using ZeroBalance.Services;

namespace ZeroBalance.Tests
{
    public class FakeHttpClient : IHttpClient
    {
        readonly HttpStatusCode _status;

        public FakeHttpClient(HttpStatusCode status)
        {
            _status = status;
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
                result.Content = new StringContent("{ \"Organisations\":[ { \"name\": \"Something\" } ] }");
            }

            return result;
        }
    }
}
