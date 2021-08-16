using System;
using System.Collections.Generic;

namespace Scheduler.Models
{
    public enum JobStatus
    {
        Normal,
        Paused,
        Complete,
        Error,
        Blocked,
        None
    }

    public class JobModel
    {
        public string JobName { get; set; }

        public string Group { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        public bool Recovery { get; set; }

        public DateTimeOffset StartTime { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? EndTime { get; set; }

        public string Cron { get; set; }

        public int Priority { get; set; } = 5;

        public IDictionary<string, object> DataMap { get; set; }

        public JobStatus? Status { get; set; }
    }
}