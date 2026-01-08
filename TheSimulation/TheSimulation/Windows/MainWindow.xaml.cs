using System.Windows;
using System.Windows.Controls;

namespace TheSimulation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly GraphicsSettings settings = new();

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
        var treeConfig = new TreeConfig
        (
            MaxCount: 50_000,
            ForestDensity: 0.6f,
            Size: 7
        );

        var pauseDuringFire = PauseFireCheckBox.IsChecked ?? true;
        var fireIntensity = FireIntensitySlider.Value; /// (Fire spread chance percent)

        var fireConfig = new FireConfig
        (
            fireIntensity,
            pauseDuringFire
        );

        var windConfig = GetWindConfigFromUI();
        var prefillConfig = GetPrefillConfigFromUI();

        var effectsConfig = new VisualEffectsConfig
        (
            settings.ShowLightning,
            settings.ShowFireParticles,
            settings.ShowSmokeParticles
        );

        return new SimulationConfig
        (
            treeConfig,
            fireConfig,
            windConfig,
            prefillConfig,
            effectsConfig,
            false // TODO : Nach der Zeit verbrannter Baum wieder verschwinden lassen
        );
    }

    private WindConfig GetWindConfigFromUI()
    {
        var randomWindDirection = RandomWindDirectionCheckBox.IsChecked ?? false;
        var windDirection = GetWindDirection();
        var randomStrength = RandomWindStrengthCheckBox.IsChecked ?? false;
        var windStrength = WindStrengthSlider.Value;

        return new WindConfig
        (
            randomWindDirection,
            windDirection,
            randomStrength,
            windStrength
        );
    }

    private PrefillConfig GetPrefillConfigFromUI()
    {
        var prefill = PrefillCheckBox.IsChecked ?? false;
        var prefillDensity = PrefillDensitySlider.Value / 100; // 0..1

        return new PrefillConfig
        (
            prefill,
            prefillDensity // Z.b: 0.5 = 50 %
        );
    }

    private WindDirection GetWindDirection()
    {
        if (RandomWindDirectionCheckBox.IsChecked == true)
        {
            return GetRandomWindDirection();
        }

        return GetParsedWindDirectionFromUI();
    }

    private WindDirection GetParsedWindDirectionFromUI()
    {
        var selectedString = ((ComboBoxItem)WindDirectionBox.SelectedItem).Content.ToString();

        return Enum.TryParse<WindDirection>(selectedString, out var windDir)
            ? windDir
            : WindDirection.North;
    }

    private static WindDirection GetRandomWindDirection()
    {
        var values = Enum.GetValues(typeof(WindDirection));
        var randomIndex = new RandomHelper().NextInt(0, values.Length);
        return (WindDirection)values.GetValue(randomIndex)!;
    }

    private void PrefillCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        // Slider aktivieren, wenn Checkbox tickt, sonst deaktivieren
        PrefillDensitySlider.IsEnabled = PrefillCheckBox.IsChecked ?? false;
    }

    private void RandomWindDirectionCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        var isRandom = RandomWindDirectionCheckBox.IsChecked ?? false;
        WindDirectionBox.IsEnabled = !isRandom;

        if (!isRandom)
        {
            return;
        }

        var randomDirection = GetRandomWindDirection();

        ShowRandomsetWinddirection(randomDirection);
    }

    private void ShowRandomsetWinddirection(WindDirection randomDirection)
    {
        foreach (ComboBoxItem item in WindDirectionBox.Items)
        {
            if (Enum.TryParse<WindDirection>(item.Content.ToString(), out var dir) &&
                dir == randomDirection)
            {
                WindDirectionBox.SelectedItem = item;
                break;
            }
        }
    }

    private void OpenGraphicsSettingsWindow(object sender, RoutedEventArgs e)
    {
        var graphicsSettingsWindow = new GraphicsWindow(settings);
        graphicsSettingsWindow.ShowDialog();
    }
}
