#!/usr/bin/env bash
#
# MonitorNBA - fix compiled bindings. Avalonia 11.3 enables them by default,
# which needs an explicit x:DataType on the window and every DataTemplate.
#
# Run from the Monitor solution root.
#
set -euo pipefail

if [ ! -f Monitor.sln ] && [ ! -f Monitor.slnx ]; then
  echo "ERROR: run this from the Monitor solution folder" >&2
  exit 1
fi

echo "Rewriting MainWindow.axaml..."

cat > MonitorNBA/MainWindow.axaml <<'CSEOF'
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:MonitorNBA.ViewModels"
        x:Class="MonitorNBA.MainWindow"
        x:DataType="vm:MainViewModel"
        Title="MonitorNBA"
        Width="900" Height="640"
        MinWidth="700" MinHeight="480">

  <DockPanel Margin="16">

    <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Spacing="12"
                Margin="0,0,0,16">
      <TextBlock Text="MonitorNBA" FontSize="22" FontWeight="SemiBold"
                 VerticalAlignment="Center" />
      <Button Content="Run all now" Command="{Binding RunAllCommand}"
              VerticalAlignment="Center" />
      <TextBlock Text="{Binding StatusLine}" Opacity="0.7"
                 VerticalAlignment="Center" />
    </StackPanel>

    <Border DockPanel.Dock="Bottom" Height="200" Margin="0,16,0,0"
            BorderBrush="#33000000" BorderThickness="1" CornerRadius="4">
      <DockPanel>
        <TextBlock DockPanel.Dock="Top" Text="Activity" FontWeight="SemiBold"
                   Margin="10,8,10,4" />
        <ScrollViewer Margin="10,0,10,10">
          <ItemsControl ItemsSource="{Binding Log}">
            <ItemsControl.ItemTemplate>
              <DataTemplate x:DataType="x:String">
                <TextBlock Text="{Binding}" FontFamily="monospace" FontSize="12"
                           Margin="0,1" TextWrapping="NoWrap" />
              </DataTemplate>
            </ItemsControl.ItemTemplate>
          </ItemsControl>
        </ScrollViewer>
      </DockPanel>
    </Border>

    <ScrollViewer>
      <ItemsControl ItemsSource="{Binding Rows}">
        <ItemsControl.ItemTemplate>
          <DataTemplate x:DataType="vm:CheckRowViewModel">
            <Border BorderBrush="#33000000" BorderThickness="1" CornerRadius="4"
                    Padding="12" Margin="0,0,0,8">
              <Grid ColumnDefinitions="8,16,2*,3*,Auto">

                <Border Grid.Column="0" Width="8" CornerRadius="4"
                        Background="{Binding StatusBrush}" />

                <StackPanel Grid.Column="2" Spacing="2">
                  <TextBlock Text="{Binding Name}" FontWeight="SemiBold" />
                  <TextBlock Text="{Binding Category}" Opacity="0.6" FontSize="12" />
                </StackPanel>

                <TextBlock Grid.Column="3" Text="{Binding MessageText}"
                           VerticalAlignment="Center" TextWrapping="Wrap"
                           Opacity="0.85" />

                <StackPanel Grid.Column="4" Spacing="2" Margin="16,0"
                            HorizontalAlignment="Right">
                  <TextBlock Text="{Binding LastRunText}" FontSize="12"
                             HorizontalAlignment="Right" />
                  <TextBlock Text="{Binding IntervalText}" FontSize="12"
                             Opacity="0.6" HorizontalAlignment="Right" />
                </StackPanel>

              </Grid>
            </Border>
          </DataTemplate>
        </ItemsControl.ItemTemplate>
      </ItemsControl>
    </ScrollViewer>

  </DockPanel>
</Window>
CSEOF

echo "Rewriting CheckRowViewModel.cs..."

cat > MonitorNBA/ViewModels/CheckRowViewModel.cs <<'CSEOF'
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
CSEOF

echo ""
echo "Building..."
echo ""
dotnet build

cat <<'MSGEOF'

==================================================================
Done. Run it with:

  dotnet run --project MonitorNBA
==================================================================
MSGEOF
