using Barbora.Core.Services;
using Hangfire;

namespace Barbora.Scheduler.Services
{
    public interface IJobsService
    {
        void RegisterAllJobs();
    }

    public class JobsService : IJobsService
    {
        public void RegisterAllJobs()
        {
            RecurringJob.AddOrUpdate<IBarboraSchedulingService>(
                "CheckAndNotify",
                x => x.CheckAndNotify(),
                Cron.Minutely
            );
        }
    }
}