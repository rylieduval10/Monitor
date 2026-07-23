using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Monitor.Core;
using MonitorNBA.ViewModels;

namespace MonitorNBA;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var settings = LoadSettings();
            desktop.MainWindow = new MainWindow(new MainViewModel(settings));
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static MonitorSettings LoadSettings()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.local.json", optional: true)
            .Build();

        var settings = new MonitorSettings();
        config.Bind(settings);

        return settings;
    }
}
