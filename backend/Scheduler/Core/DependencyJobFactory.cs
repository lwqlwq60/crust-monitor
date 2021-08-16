using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using Scheduler.Jobs.Core;

namespace Scheduler.Core
{
    public class DependencyJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ConcurrentDictionary<IJob, IServiceScope> _scopes =
            new ConcurrentDictionary<IJob, IServiceScope>();

        public DependencyJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobScope = _serviceProvider.CreateScope();
            try
            {
                var job = jobScope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
                _scopes.TryAdd(job, jobScope);
                return job;
            }
            catch (InvalidOperationException)
            {
                jobScope.Dispose();
                return new ExceptionJob(new Exception($"can not find {bundle.JobDetail.JobType.FullName}."));
            }
        }

        public void ReturnJob(IJob job)
        {
            if (_scopes.TryRemove(job, out var jobScope)) jobScope.Dispose();
        }
    }
}