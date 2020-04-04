using Barbora.Core.Services;
using Barbora.Scheduler.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Barbora.Scheduler.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterDefaultContainer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IJobsService, JobsService>();
            serviceCollection.AddSingleton<IBarboraSchedulingService, BarboraSchedulingService>();
        }
    }
}