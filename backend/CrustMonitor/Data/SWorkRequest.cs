using System.Text.Json.Serialization;

namespace CrustMonitor.Data
{
    public class SWorkRequest
    {
        [JsonPropertyName("row")] public int Row { get; set; } = 1;

        [JsonPropertyName("page")] public int Page { get; set; } = 0;

        [JsonPropertyName("signed")] public string Signed { get; set; } = "signed";

        [JsonPropertyName("address")] public string Address { get; set; }

        [JsonPropertyName("module")] public string Module { get; set; } = "swork";

        [JsonPropertyName("call")] public string Call { get; set; } = string.Empty;

        [JsonPropertyName("no_params")] public bool NoParams { get; set; }
    }
}