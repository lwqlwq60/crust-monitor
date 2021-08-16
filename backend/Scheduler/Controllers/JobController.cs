using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl.Matchers;
using Scheduler.Models;

namespace Scheduler.Controllers
{
    [Route("api/jobs")]
    public class JobController : Controller
    {
        private readonly IScheduler _scheduler;

        public JobController(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] JobModel jobModel)
        {
            IJobDetail BuildJob(JobBuilder jobBuilder)
            {
                return jobBuilder
                    .OfType(Type.GetType(jobModel.Type, true))
                    .WithIdentity(jobModel.JobName, jobModel.Group)
                    .WithDescription(jobModel.Description)
                    .SetJobData(new JobDataMap(jobModel.DataMap))
                    .RequestRecovery(jobModel.Recovery)
                    .Build();
            }

            await _scheduler.AddJob(BuildJob(JobBuilder.Create().StoreDurably()), false);

            var triggerBuilder = TriggerBuilder.Create()
                .WithIdentity(new TriggerKey($"{jobModel.JobName}_trigger", $"{jobModel.Group}_trigger"))
                .ForJob(jobModel.JobName, jobModel.Group)
                .UsingJobData(new JobDataMap(jobModel.DataMap))
                .WithDescription(jobModel.Description)
                .WithPriority(jobModel.Priority)
                .StartAt(jobModel.StartTime)
                .EndAt(jobModel.EndTime)
                .WithCronSchedule(jobModel.Cron, x => x.WithMisfireHandlingInstructionDoNothing());

            var trigger = triggerBuilder.Build();
            await _scheduler.ScheduleJob(trigger);

            return Ok();
        }


        [HttpPost("{jobGroup}/{jobName}/pause")]
        public async Task<IActionResult> PauseAsync(string jobGroup, string jobName)
        {
            try
            {
                var triggerKey = new TriggerKey($"{jobName}_trigger", $"{jobGroup}_trigger");
                await _scheduler.PauseTrigger(triggerKey);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("{jobGroup}/{jobName}/resume")]
        public async Task<IActionResult> ResumeAsync(string jobGroup, string jobName)
        {
            try
            {
                var triggerKey = new TriggerKey($"{jobName}_trigger", $"{jobGroup}_trigger");
                await _scheduler.ResumeTrigger(triggerKey);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        private async Task<(bool Exists, JobModel JobModel)> TryGetByKeyAsync(JobKey jobKey)
        {
            static string GetCronExpression(ITrigger trigger)
            {
                if (trigger is ICronTrigger cr)
                    return cr.CronExpressionString;
                return null;
            }

            var jobDetail = await _scheduler.GetJobDetail(jobKey);
            if (jobDetail != null)
            {
                var triggerKey = new TriggerKey($"{jobKey.Name}_trigger",
                    $"{jobKey.Group}_trigger");
                var trigger =
                    await _scheduler.GetTrigger(triggerKey);
                if (trigger != null)
                {
                    var state = await _scheduler.GetTriggerState(triggerKey);
                    var jobModel = new JobModel
                    {
                        JobName = jobDetail.Key.Name,
                        Group = jobDetail.Key.Group,
                        Description = jobDetail.Description,
                        DataMap = jobDetail.JobDataMap,
                        Type = jobDetail.JobType.FullName,
                        Recovery = jobDetail.RequestsRecovery,
                        StartTime = trigger.StartTimeUtc,
                        EndTime = trigger.EndTimeUtc,
                        Priority = trigger.Priority,
                        Cron = GetCronExpression(trigger),
                        Status = (JobStatus) (int) state
                    };
                    return (true, jobModel);
                }
            }

            return (false, default);
        }

        [HttpGet("{jobGroup}/{jobName}")]
        public async Task<IActionResult> GetByKeyAsync(string jobGroup, string jobName)
        {
            var (exists, jobModel) = await TryGetByKeyAsync(new JobKey(jobName, jobGroup));
            if (!exists)
                return BadRequest();
            return Json(jobModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var jobKeys = (await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup())).OrderBy(x => x.ToString());
            var models = new List<JobModel>();
            foreach (var jobKey in jobKeys)
            {
                var (exists, jobModel) = await TryGetByKeyAsync(jobKey);
                if (exists)
                    models.Add(jobModel);
            }

            return Json(models);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromBody] JobModel jobModel)
        {
            var jobKey = new JobKey(jobModel.JobName, jobModel.Group);
            var triggerKey = new TriggerKey($"{jobKey.Name}_trigger", $"{jobKey.Group}_trigger");
            if (!await _scheduler.UnscheduleJob(triggerKey))
                return BadRequest();
            if (!await _scheduler.DeleteJob(jobKey))
                return BadRequest();

            return NoContent();
        }
    }
}