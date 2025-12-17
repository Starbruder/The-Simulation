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
        var windStrength = (float)WindStrengthSlider.Value;

        var treeConfig = new TreeConfig
        (
            50_000,
            0.6f,
            7
        );

        var fireConfig = new FireConfig
        (
            fireIntensity,
            true
        );

        var windConfig = new WindConfig
        (
            windDiriction,
            windStrength
        );

        var config = new SimulationConfig
        (
            treeConfig,
            fireConfig,
            windConfig,
            false,
            false
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
