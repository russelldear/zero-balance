using System;
using Xunit;
using ZeroBalance.Services;

namespace ZeroBalance.Tests
{
    public class LiveTest
    {
        [Fact(Skip = "true")] // Get access token from Cloudwatch to run this
        public void live_test()
        {
            var result = new XeroService("987ece2d9d45751dcb2f86bb96192362aae84aaf3dc080dab6e77454466ceeab").GetConnections();
        }
    }
}
