using System;

namespace Scheduler.Models
{
    public class SchedulerModel
    {
        public string SchedulerName { get; set; }

        public string SchedulerInstanceId { get; set; }

        public string SchedulerType { get; set; }

        public bool SchedulerRemote { get; set; }

        public bool Started { get; set; }

        public bool InStandbyMode { get; set; }

        public bool Shutdown { get; set; }

        public string JobStoreType { get; set; }

        public string ThreadPoolType { get; set; }

        public int ThreadPoolSize { get; set; }

        public DateTimeOffset? RunningSince { get; set; }

        public int NumberOfJobsExecuted { get; set; }
        public int NumberOfJobsFailed { get; set; }
        public int NumberOfJobsExecuting { get; set; }

        public bool JobStoreSupportsPersistence { get; set; }

        public bool JobStoreClustered { get; set; }
    }
}