namespace Monitor.Core;

public class CheckResult
{
    public bool Success { get; set; } = true;

    /// <summary>One line, shown in the status list.</summary>
    public string Message { get; set; } = "";

    /// <summary>Optional longer text, shown in the log.</summary>
    public string? Details { get; set; }

    public DateTime RanAt { get; set; } = DateTime.Now;
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// True when the check ran fine but found something worth a human looking.
    /// Distinct from Success=false, which means the check itself broke.
    /// </summary>
    public bool NeedsAttention { get; set; }
}
