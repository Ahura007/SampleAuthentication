using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SampleAuthentication.Activators;
using SampleAuthentication.DatabaseContext;

namespace SampleAuthentication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hostAddresses = ReadHostAddresses();

            BuildWebHost(args).UseUrls(hostAddresses.HostAddress).Build().Seed().Run();
        }

        public static IWebHostBuilder BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }

        private static HostAddresses ReadHostAddresses()
        {
            var appSettings = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json")
                .Build()
                .GetSection(nameof(HostAddresses))
                .Get<HostAddresses>();
            return appSettings;
        }
    }
}