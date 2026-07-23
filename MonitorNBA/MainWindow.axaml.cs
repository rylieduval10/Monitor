using Avalonia.Controls;
using MonitorNBA.ViewModels;

namespace MonitorNBA;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.Start();

        Closed += (_, _) => viewModel.Stop();
    }
}
