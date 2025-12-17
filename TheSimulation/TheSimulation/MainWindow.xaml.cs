using System.Windows;
using System.Windows.Controls;

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

        var windDiriction = GetWindDirection();

        var config = new SimulationConfig
        (
            50000,
            0.6f,
            7,
            fireIntensity,
            false,
            false,
            true,
            windDiriction,
            0.8f
        );

        new SimulationWindow(config).Show();
    }

    private WindDirection GetWindDirection()
    {
        var selectedString = ((ComboBoxItem)WindDirectionBox.SelectedItem).Content.ToString();

        return Enum.TryParse<WindDirection>(selectedString, out var windDir)
            ? windDir
            : WindDirection.North;
    }
}
