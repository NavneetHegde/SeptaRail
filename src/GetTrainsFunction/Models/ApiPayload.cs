namespace GetTrainsFunction;

public class ApiRequest
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}

public class ApiResponse
{
    public string orig_train { get; set; } = string.Empty;
    public string orig_line { get; set; } = string.Empty;
    public string orig_departure_time { get; set; } = string.Empty;
    public string orig_arrival_time { get; set; } = string.Empty;
    public string orig_delay { get; set; } = string.Empty;
    public string isdirect { get; set; } = string.Empty;
}

