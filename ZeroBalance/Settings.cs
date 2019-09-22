using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ZeroBalance
{
    public class Settings
    {
        private readonly IConfigurationRoot Configuration;

        public Settings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        public string XeroBaseUrl
        {
            get
            {
                if(!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Constants.BaseUrlKey)))
                {
                    return Environment.GetEnvironmentVariable(Constants.BaseUrlKey);
                }

                return Configuration[Constants.BaseUrlKey];
            }
        }
    }
}
