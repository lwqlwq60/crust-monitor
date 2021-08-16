namespace Scheduler.Jobs
{
    public class JobSettings
    {
        public int? HttpClientGetTimeout { get; set; }
        public int? HttpClientPostTimeout { get; set; }
        public int? HandledEventsAllowedBeforeBreaking { get; set; }
        public int? DurationOfBreak { get; set; }
    }
}