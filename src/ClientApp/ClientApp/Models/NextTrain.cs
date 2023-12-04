using System.Text.Json.Serialization;

namespace SeptaRail.ClientApp.Models;

public class NextTrain
{
    [JsonPropertyName("orig_train")]
    public string orig_train { get; set; }

    [JsonPropertyName("orig_line")]
    public string orig_line { get; set; }

    [JsonPropertyName("orig_departure_time")]
    public string orig_departure_time { get; set; }

    [JsonPropertyName("orig_arrival_time")]
    public string orig_arrival_time { get; set; }

    [JsonPropertyName("orig_delay")]
    public string orig_delay { get; set; }

    [JsonPropertyName("isdirect")]
    public string isdirect { get; set; }
}

