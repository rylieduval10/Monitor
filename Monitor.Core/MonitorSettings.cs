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

    /// <summary>
    /// When set above zero, this wins over IntervalMinutes. For in-season
    /// pulls that need to run faster than once a minute.
    /// </summary>
    public int IntervalSeconds { get; set; }

    /// <summary>The effective interval, whichever was set.</summary>
    public TimeSpan Interval => IntervalSeconds > 0
        ? TimeSpan.FromSeconds(IntervalSeconds)
        : TimeSpan.FromMinutes(Math.Max(1, IntervalMinutes));
}
