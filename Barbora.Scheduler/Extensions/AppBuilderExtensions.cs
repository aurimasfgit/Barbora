using Barbora.Scheduler.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Barbora.Scheduler
{
    public static class AppBuilderExtensions
    {
        public static void RegisterAllJobs(this IApplicationBuilder app)
        {
            var jobsService = app.ApplicationServices.GetService<IJobsService>();

            if (jobsService == null)
                throw new ArgumentNullException("jobsService");

            jobsService.RegisterAllJobs();
        }
    }
}