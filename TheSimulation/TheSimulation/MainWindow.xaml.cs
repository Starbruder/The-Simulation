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
        var config = GetSimulationConfigFromUI();
        new SimulationWindow(config).Show();
    }

    private SimulationConfig GetSimulationConfigFromUI()
    {
        var fireIntensity = (float)FireIntensitySlider.Value; /// (Fire spread chance percent)

        var windDirection = GetParsedWindDirection();
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
            RandomWindDirectionCheckBox.IsChecked ?? false,
            windDirection,
            windStrength
        );

        var prefill = PrefillCheckBox.IsChecked ?? false;
        var prefillDensity = (float)PrefillDensitySlider.Value / 100f; // 0..1

        var prefillConfig = new PrefillConfig
        (
            prefill,
            prefillDensity // Z.b: 0.5 = 50 %
        );

        var effectsConfig = new VisualEffectsConfig
        (
            false,
            true,
            false
        );

        return new SimulationConfig
        (
            treeConfig,
            fireConfig,
            windConfig,
            prefillConfig,
            effectsConfig,
            false
        );
    }

    private WindDirection GetParsedWindDirection()
    {
        var selectedString = ((ComboBoxItem)WindDirectionBox.SelectedItem).Content.ToString();

        return Enum.TryParse<WindDirection>(selectedString, out var windDir)
            ? windDir
            : WindDirection.North;
    }

    private void PrefillCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        // Slider aktivieren, wenn Checkbox tickt, sonst deaktivieren
        PrefillDensitySlider.IsEnabled = PrefillCheckBox.IsChecked ?? false;
    }

    private void RandomWindDirectionCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        WindDirectionBox.IsEnabled = !(RandomWindDirectionCheckBox.IsChecked ?? false);
    }
}
