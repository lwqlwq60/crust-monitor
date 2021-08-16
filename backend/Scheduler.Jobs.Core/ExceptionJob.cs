using System;
using System.Threading.Tasks;
using Quartz;

namespace Scheduler.Jobs.Core
{
    public class ExceptionJob : IJob
    {
        private readonly Exception _exception;

        public ExceptionJob(Exception exception)
        {
            _exception = exception;
        }

        public Task Execute(IJobExecutionContext context)
        {
            throw _exception;
        }
    }
}