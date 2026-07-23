using Avalonia.Media;
using Monitor.Core;

namespace MonitorNBA.ViewModels;

public class CheckRowViewModel : ViewModelBase
{
    private static readonly IBrush ErrorBrush = new SolidColorBrush(Color.Parse("#c0392b"));
    private static readonly IBrush AttentionBrush = new SolidColorBrush(Color.Parse("#d68910"));
    private static readonly IBrush OkBrush = new SolidColorBrush(Color.Parse("#1e8449"));
    private static readonly IBrush IdleBrush = new SolidColorBrush(Color.Parse("#7f8c8d"));

    public CheckRowViewModel(MonitorCheck check)
    {
        Check = check;
    }

    public MonitorCheck Check { get; }

    public string Name => Check.Name;

    public string Category => Check.Category;

    public string IntervalText => Check.Interval.TotalMinutes >= 1
        ? $"every {Check.Interval.TotalMinutes:0} min"
        : $"every {Check.Interval.TotalSeconds:0} sec";

    public string LastRunText => Check.LastRun == null
        ? "never run"
        : Check.LastRun.Value.ToString("h:mm:ss tt");

    public string StatusText
    {
        get
        {
            if (Check.LastResult == null) return "waiting";
            if (!Check.LastResult.Success) return "error";
            return Check.LastResult.NeedsAttention ? "attention" : "ok";
        }
    }

    public IBrush StatusBrush => StatusText switch
    {
        "error" => ErrorBrush,
        "attention" => AttentionBrush,
        "ok" => OkBrush,
        _ => IdleBrush
    };

    public string MessageText => Check.LastResult?.Message ?? "";

    public void Refresh()
    {
        Raise(nameof(LastRunText));
        Raise(nameof(StatusText));
        Raise(nameof(StatusBrush));
        Raise(nameof(MessageText));
    }
}
