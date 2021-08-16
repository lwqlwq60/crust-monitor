using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Scheduler.Jobs.Script
{
    public class RuntimeScriptJob : IJob
    {
        private readonly ILogger _logger;
        private readonly ScriptCache _scriptCache;
        private readonly ScriptOptions _scriptOptions;
        protected readonly ScriptContext ScriptContext;

        public RuntimeScriptJob(
            ILogger<RuntimeScriptJob> logger,
            ScriptCache scriptCache,
            ScriptOptions scriptOptions)
        {
            _logger = logger;
            _scriptCache = scriptCache;
            _scriptOptions = scriptOptions;
            ScriptContext = new ScriptContext {Logger = logger};
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            if (!dataMap.ContainsKey("script"))
                throw new JobExecutionException("Can not find parameter :script.");
            
            var code = dataMap.GetString("script");
            if (!_scriptCache.TryGetValue(code, out var scriptRunner))
            {
                scriptRunner = CSharpScript.Create(code, _scriptOptions,
                    typeof(ScriptContext)).CreateDelegate();
                _scriptCache.TryAdd(code, scriptRunner);
            }

            await scriptRunner(ScriptContext);
        }
    }
}