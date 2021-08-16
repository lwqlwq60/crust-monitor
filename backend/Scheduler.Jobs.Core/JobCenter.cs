using Microsoft.Extensions.DependencyInjection;
using Scheduler.Jobs.Script;

namespace Scheduler.Jobs.Core
{
    public static class JobCenter
    {
        public static void AddQuartzJob(this IServiceCollection serviceCollection)
        {
            //set built-in job.
            serviceCollection.AddBuiltInJob();
            //set normal script job.
            serviceCollection.AddScriptJob();
        }
    }
}