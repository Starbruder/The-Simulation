using System.Windows;

namespace TheSimulation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        UIHelper.InitializeWindowIcon(this);
    }

    private void StartSimulation_Click(object sender, RoutedEventArgs e)
    {
        var config = new SimulationConfig
        (
            50000,
            0.6f,
            7,
            75f,
            false,
            false,
            true
        );

        new SimulationWindow(config).Show();
    }
}
