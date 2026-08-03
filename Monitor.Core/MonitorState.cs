using System.Text.Json;

namespace Monitor.Core;

/// <summary>
/// What the monitor remembers between runs: which checks are paused, and what
/// state each was last in.
///
/// Without this, restarting un-pauses everything and loses the knowledge that
/// something was broken - so a check that failed before the restart would come
/// back up silently instead of reporting that it recovered.
/// </summary>
public class MonitorState
{
    public Dictionary<string, bool> Paused { get; set; } = new();

    /// <summary>Ok, Attention or Failed, keyed by check name.</summary>
    public Dictionary<string, string> LastState { get; set; } = new();
}

public class MonitorStateStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _path;
    private readonly object _lock = new();

    public MonitorStateStore(string path)
    {
        _path = path;
    }

    /// <summary>Sits next to the executable, so each monitor app keeps its own.</summary>
    public static MonitorStateStore Default(string fileName = "monitor-state.json")
        => new(Path.Combine(AppContext.BaseDirectory, fileName));

    /// <summary>Never throws. A corrupt or missing file just means starting fresh.</summary>
    public MonitorState Load()
    {
        try
        {
            if (!File.Exists(_path)) return new MonitorState();

            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<MonitorState>(json) ?? new MonitorState();
        }
        catch
        {
            return new MonitorState();
        }
    }

    /// <summary>Never throws - failing to save state must not take the monitor down.</summary>
    public void Save(MonitorState state)
    {
        try
        {
            lock (_lock)
            {
                File.WriteAllText(_path, JsonSerializer.Serialize(state, Options));
            }
        }
        catch
        {
            // Ignored on purpose.
        }
    }
}
