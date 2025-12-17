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
        IconVisualizer.InitializeWindowIcon(this);
    }

    private void StartSimulation_Click(object sender, RoutedEventArgs e)
    {
        var fireIntensity = (float)FireIntensitySlider.Value; /// (Fire spread chance percent)

		var config = new SimulationConfig
        (
            50000,
            0.6f,
            7,
            fireIntensity,
            false,
            false,
            true,
            WindDirection.East,
            0.8f
        );

        new SimulationWindow(config).Show();
    }
}
