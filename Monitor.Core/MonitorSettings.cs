namespace Monitor.Core;

public class MonitorSettings
{
    public TwitterApiSettings TwitterApi { get; set; } = new();
}

public class TwitterApiSettings
{
    public string BaseUrl { get; set; } = "https://twitter.rotomonster.com";
    public string ApiKey { get; set; } = "";

    /// <summary>List to pull for this app's sport.</summary>
    public long ListId { get; set; }

    public int IntervalMinutes { get; set; } = 10;
}
