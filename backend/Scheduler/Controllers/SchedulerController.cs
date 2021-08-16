using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Plugins.RecentHistory;
using Scheduler.Models;

namespace Scheduler.Controllers
{
    [Route("api/scheduler")]
    public class SchedulerController : Controller
    {
        private readonly IScheduler _scheduler;

        public SchedulerController(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var model = new SchedulerModel();
            var histStore = _scheduler.Context.GetExecutionHistoryStore();
            var metaData = await _scheduler.GetMetaData();
            var currentlyExecutingJobs = await _scheduler.GetCurrentlyExecutingJobs();
            model.Shutdown = metaData.Shutdown;
            model.Started = metaData.Started;
            model.RunningSince = metaData.RunningSince;
            model.SchedulerName = metaData.SchedulerName;
            model.SchedulerRemote = metaData.SchedulerRemote;
            model.SchedulerType = metaData.SchedulerType.Name;
            model.InStandbyMode = metaData.InStandbyMode;
            model.JobStoreClustered = metaData.JobStoreClustered;
            model.JobStoreType = metaData.JobStoreType.Name;
            model.SchedulerInstanceId = metaData.SchedulerInstanceId;
            model.ThreadPoolSize = metaData.ThreadPoolSize;
            model.ThreadPoolType = metaData.ThreadPoolType.Name;
            model.JobStoreSupportsPersistence = metaData.JobStoreSupportsPersistence;
            model.NumberOfJobsExecuted = metaData.NumberOfJobsExecuted;
            if (histStore != null)
            {
                model.NumberOfJobsExecuted = await histStore.GetTotalJobsExecuted();
                model.NumberOfJobsFailed = await histStore.GetTotalJobsFailed();
            }

            model.NumberOfJobsExecuting = currentlyExecutingJobs.Count;
            return Json(model);
        }

        [HttpGet("history/{limit}")]
        public async Task<IActionResult> GetHistory([FromRoute]int limit)
        {
            var store = _scheduler.Context.GetExecutionHistoryStore();
            return Json(await store.FilterLast(limit));
        }
    }
}