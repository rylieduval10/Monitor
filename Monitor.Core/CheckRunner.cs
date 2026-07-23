namespace Monitor.Core;

public class CheckCompletedEventArgs : EventArgs
{
    public MonitorCheck Check { get; init; } = null!;
    public CheckResult Result { get; init; } = null!;
}

/// <summary>
/// Owns the schedule. Wakes every few seconds, runs whatever is due, and
/// raises CheckCompleted so the UI can update.
/// </summary>
public class CheckRunner
{
    private readonly List<MonitorCheck> _checks;

    public CheckRunner(IEnumerable<MonitorCheck> checks)
        => _checks = checks.ToList();

    public IReadOnlyList<MonitorCheck> Checks => _checks;

    public event EventHandler<CheckCompletedEventArgs>? CheckCompleted;

    public TimeSpan TickInterval { get; set; } = TimeSpan.FromSeconds(10);

    public async Task RunOneAsync(MonitorCheck check, CancellationToken ct = default)
    {
        var result = await check.RunAsync(ct);
        CheckCompleted?.Invoke(this, new CheckCompletedEventArgs
        {
            Check = check,
            Result = result
        });
    }

    public async Task RunAllAsync(CancellationToken ct = default)
    {
        foreach (var check in _checks.Where(c => c.IsEnabled))
        {
            if (ct.IsCancellationRequested) return;
            await RunOneAsync(check, ct);
        }
    }

    /// <summary>Runs until cancelled. One slow or failing check never stops the rest.</summary>
    public async Task RunLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TickInterval);

        while (!ct.IsCancellationRequested)
        {
            var now = DateTime.Now;

            foreach (var check in _checks)
            {
                if (ct.IsCancellationRequested) return;
                if (!check.IsDue(now)) continue;

                try
                {
                    await RunOneAsync(check, ct);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(ct)) return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
