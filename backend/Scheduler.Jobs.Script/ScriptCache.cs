using System.Collections.Concurrent;
using Microsoft.CodeAnalysis.Scripting;

namespace Scheduler.Jobs.Script
{
    //strong type for di-container.
    public sealed class ScriptCache : ConcurrentDictionary<string, ScriptRunner<object>>
    {
    }
}