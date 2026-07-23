using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Monitor.Core;
using Monitor.Core.Checks;

namespace MonitorNBA.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly CheckRunner _runner;
    private readonly CancellationTokenSource _cts = new();

    private bool _isRunning;
    private string _statusLine = "Idle.";

    public MainViewModel(MonitorSettings settings)
    {
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

        var checks = new List<MonitorCheck>
        {
            new TwitterIngestCheck(http, settings.TwitterApi),
            new ApiHealthCheck(http, "Twitter API",
                $"{settings.TwitterApi.BaseUrl.TrimEnd('/')}/health")
        };

        _runner = new CheckRunner(checks);
        _runner.CheckCompleted += OnCheckCompleted;

        Rows = new ObservableCollection<CheckRowViewModel>(
            checks.Select(c => new CheckRowViewModel(c)));

        RunAllCommand = new RelayCommand(async void () => await RunAllAsync(),
            () => !IsRunning);
    }

    public ObservableCollection<CheckRowViewModel> Rows { get; }

    public ObservableCollection<string> Log { get; } = new();

    public RelayCommand RunAllCommand { get; }

    public string Title => "MonitorNBA";

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (Set(ref _isRunning, value)) RunAllCommand.RaiseCanExecuteChanged();
        }
    }

    public string StatusLine
    {
        get => _statusLine;
        private set => Set(ref _statusLine, value);
    }

    /// <summary>Starts the scheduled loop. Called once at startup.</summary>
    public void Start()
    {
        Append("Monitor started.");
        _ = _runner.RunLoopAsync(_cts.Token);
    }

    public void Stop() => _cts.Cancel();

    public async Task RunAllAsync()
    {
        IsRunning = true;
        StatusLine = "Running all checks...";

        try
        {
            await _runner.RunAllAsync(_cts.Token);
            StatusLine = $"Last full run at {DateTime.Now:h:mm:ss tt}.";
        }
        finally
        {
            IsRunning = false;
        }
    }

    private void OnCheckCompleted(object? sender, CheckCompletedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var row = Rows.FirstOrDefault(r => r.Check == e.Check);
            row?.Refresh();

            var prefix = e.Result.Success
                ? (e.Result.NeedsAttention ? "!" : " ")
                : "X";

            Append($"{prefix} [{e.Result.RanAt:h:mm:ss tt}] {e.Check.Name}: {e.Result.Message}");
        });
    }

    private void Append(string line)
    {
        Log.Insert(0, line);

        while (Log.Count > 300)
        {
            Log.RemoveAt(Log.Count - 1);
        }
    }
}
