namespace CrustMonitor.Data
{
    public class CrustNode
    {
        public string Ip { get; set; }
        public bool Online { get; set; } = false;
        public string ChainStatus { get; set; } = "Unknown";
        public string ApiStatus { get; set; } = "Unknown";
        public string SWorkerStatus { get; set; } = "Unknown";
        public string SWorkerAStatus { get; set; } = "Unknown";
        public string SWorkerBStatus { get; set; } = "Unknown";
        public string SManagerStatus { get; set; } = "Unknown";
        public string IpfsStatus { get; set; } = "Unknown";
    }
}