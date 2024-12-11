using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Library.Utils;

namespace EventTicketsManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                EnvReader.Load(".env");
            } catch (FileNotFoundException)
            {
                // Do nothing
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
