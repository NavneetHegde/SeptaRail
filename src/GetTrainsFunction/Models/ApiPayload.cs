namespace GetTrainsFunction;

public class ApiRequest
{
    public string From { get; set; }
    public string To { get; set; }
}

public class ApiResponse
{
    public string orig_train { get; set; }
    public string orig_line { get; set; }
    public string orig_departure_time { get; set; }
    public string orig_arrival_time { get; set; }
    public string orig_delay { get; set; }
    public string isdirect { get; set; }
}

