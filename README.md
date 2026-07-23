# Monitor

Desktop monitoring apps for the Monster fantasy sports platforms.
Replaces the collection of separate periodic-check clients with one
app per sport.

## Projects

- **Monitor.Core** — shared library. Check base class, result type,
  and the scheduler that runs checks on their own intervals.
- **MonitorNBA** — Avalonia desktop app for basketball.
- **MonitorMLB** — same shape, not built yet.

## Building

Needs the .NET 10 SDK. Nothing else — Avalonia comes down through
NuGet on restore.

    dotnet build
    dotnet run --project MonitorNBA

## Configuration

`MonitorNBA/appsettings.json` holds the non-secret settings. The API
key goes in `appsettings.local.json` next to it, which is gitignored:

    {
      "TwitterApi": {
        "ApiKey": "your-key-here"
      }
    }

## Adding a check

Subclass `MonitorCheck` in `Monitor.Core/Checks`. A check is a name,
an interval, and an `ExecuteAsync` returning `Ok`, `Attention`, or
`Failed`. The runner handles scheduling, timing, and error trapping,
so a check only contains what's unique to it.

See `TwitterIngestCheck` for a worked example.
