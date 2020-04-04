using Barbora.Core.Models;
using Barbora.Scheduler.Extensions;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace Barbora.Scheduler
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage()
            );

            services.Configure<Credentials>(Configuration.GetSection("Credentials"));

            services.AddHangfireServer();
            services.RegisterDefaultContainer();
            services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseHangfireDashboard();

            app.RegisterAllJobs();
        }
    }
}