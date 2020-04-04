using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace Barbora.Scheduler
{
    class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
        }

        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
    }
}