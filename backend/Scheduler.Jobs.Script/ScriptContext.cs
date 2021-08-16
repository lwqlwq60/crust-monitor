using Microsoft.Extensions.Logging;

namespace Scheduler.Jobs.Script
{
    public class ScriptContext
    {
        public ILogger Logger { get; set; }

        public object Context { get; set; }
    }
}