using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Scheduler.Jobs.Script
{
    public static class ApplicationBuilderExtensions
    {
        public static void AddScriptJob(this IServiceCollection serviceCollection)
        {
            //set script cache.
            serviceCollection.AddSingleton<ScriptCache>();
            //set script option
            serviceCollection.AddSingleton(ScriptOptions.Default.AddReferences(typeof(ILogger).Assembly));
            serviceCollection.AddScoped<RuntimeScriptJob>();
        }
    }
}