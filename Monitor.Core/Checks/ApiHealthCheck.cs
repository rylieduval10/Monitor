namespace Monitor.Core.Checks;

/// <summary>Plain reachability check for any endpoint that answers a GET.</summary>
public class ApiHealthCheck : MonitorCheck
{
    private readonly HttpClient _http;
    private readonly string _url;
    private readonly string _label;

    public ApiHealthCheck(HttpClient http, string label, string url)
    {
        _http = http;
        _label = label;
        _url = url;
        Interval = TimeSpan.FromMinutes(5);
    }

    public override string Name => $"{_label} health";

    public override string Category => "Health";

    protected override async Task<CheckResult> ExecuteAsync(CancellationToken ct)
    {
        var response = await _http.GetAsync(_url, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        return response.IsSuccessStatusCode
            ? Ok("Reachable.", body)
            : Failed($"Returned {(int)response.StatusCode}.", body);
    }
}
