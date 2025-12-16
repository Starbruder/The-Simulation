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
        var fireIntensity = (float)FireIntensitySlider.Value;

        var config = new SimulationConfig
        (
            50000,
            0.6f,
            7,
            fireIntensity,
            false,
            false,
            true
        );

        new SimulationWindow(config).Show();
    }
}
