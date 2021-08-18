using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CrustMonitor.Data
{
    public class Workload
    {
        public WorkloadFiles Files { get; set; }
        public WorkloadSrd Srd { get; set; }
    }

    public class WorkloadFiles
    {
        public WorkloadFile Lost { get; set; }
        public WorkloadFile Pending { get; set; }
        public WorkloadFile Valid { get; set; }
    }

    public class WorkloadFile
    {
        [JsonPropertyName("num")] public int Number { get; set; }
        public int Size { get; set; }
    }

    public class WorkloadSrd
    {
        [JsonPropertyName("srd_complete")] public int SrdComplete { get; set; }

        [JsonPropertyName("srd_remaining_task")]
        public int SrdRemainingTask { get; set; }

        [JsonPropertyName("disk_available_for_srd")]
        public int DiskAvailableForSrd { get; set; }

        [JsonPropertyName("disk_available")] public int DiskAvailable { get; set; }
        [JsonPropertyName("srd_volume")] public int DiskVolume { get; set; }

        [JsonPropertyName("sys_disk_available")]
        public int SysDiskAvailable { get; set; }

        [JsonPropertyName("srd_detail")] public IDictionary<string, SrdDetail> SrdDetail { get; set; }
    }

    public class SrdDetail
    {
        public int Srd { get; set; }
        [JsonPropertyName("srd_avail")] public int SrdAvailable { get; set; }
        [JsonPropertyName("avail")] public int Available { get; set; }
        [JsonPropertyName("volumn")] public int Volume { get; set; }
    }
}