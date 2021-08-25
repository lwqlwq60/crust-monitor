// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class AccountDisplay
{
    [JsonPropertyName("address")] public string Address { get; set; }

    [JsonPropertyName("display")] public string Display { get; set; }

    [JsonPropertyName("judgements")] public object Judgements { get; set; }

    [JsonPropertyName("account_index")] public string AccountIndex { get; set; }

    [JsonPropertyName("identity")] public bool Identity { get; set; }

    [JsonPropertyName("parent")] public object Parent { get; set; }
}

public class Extrinsic
{
    [JsonPropertyName("block_timestamp")] public int BlockTimestamp { get; set; }

    [JsonPropertyName("block_num")] public int BlockNum { get; set; }

    [JsonPropertyName("extrinsic_index")] public string ExtrinsicIndex { get; set; }

    [JsonPropertyName("call_module_function")]
    public string CallModuleFunction { get; set; }

    [JsonPropertyName("call_module")] public string CallModule { get; set; }

    [JsonPropertyName("params")] public string Params { get; set; }

    [JsonPropertyName("account_id")] public string AccountId { get; set; }

    [JsonPropertyName("account_index")] public string AccountIndex { get; set; }

    [JsonPropertyName("signature")] public string Signature { get; set; }

    [JsonPropertyName("nonce")] public int Nonce { get; set; }

    [JsonPropertyName("extrinsic_hash")] public string ExtrinsicHash { get; set; }

    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("fee")] public string Fee { get; set; }

    [JsonPropertyName("from_hex")] public string FromHex { get; set; }

    [JsonPropertyName("finalized")] public bool Finalized { get; set; }

    [JsonPropertyName("account_display")] public AccountDisplay AccountDisplay { get; set; }
}

public class Data
{
    [JsonPropertyName("count")] public int Count { get; set; }

    [JsonPropertyName("extrinsics")] public List<Extrinsic> Extrinsics { get; set; }
}

public class ExtrinsicModel
{
    [JsonPropertyName("code")] public int Code { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; }

    [JsonPropertyName("generated_at")] public int GeneratedAt { get; set; }

    [JsonPropertyName("data")] public Data Data { get; set; }
}