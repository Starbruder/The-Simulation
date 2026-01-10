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
        var regrowForest = GrowForestCheckBox.IsChecked ?? true;

        var treeConfig = new TreeConfig
        (
            MaxCount: 50_000,
            ForestDensity: 0.6f,
            Size: 7,
            AllowRegrowForest: regrowForest
        );

        var pauseDuringFire = PauseFireCheckBox.IsChecked ?? true;
        var fireIntensity = FireIntensitySlider.Value; // (Fire spread chance percent)

        var fireConfig = new FireConfig
        (
            fireIntensity,
            pauseDuringFire
        );

        var environmentConfig = GetEnvironmentConfigFromUI();

        var prefillConfig = GetPrefillConfigFromUI();

        var effectsConfig = new VisualEffectsConfig
        (
            settings.ShowLightning,
            settings.ShowFireParticles,
            settings.ShowSmokeParticles,
            settings.ShowBurnedDownTrees // TODO : Nach der Zeit verbrannter Baum wieder verschwinden lassen
        );

        return new
        (
            treeConfig,
            fireConfig,
            environmentConfig,
            prefillConfig,
            effectsConfig
        );
    }

    private EnvironmentConfig GetEnvironmentConfigFromUI()
    {
        var airHumidityPercentage = AirHumiditySlider.Value / 100;
        var airTemperatureCelsius = AirTemperatureSlider.Value;

        var atmosphereConfig = new AtmosphereConfig
        (
            // Air Humidity Percentage: 0.3 = trocken, 0.7 = feucht ( % )
            (float)airHumidityPercentage,
            // Air Temperature Celsius ( °C )
            (float)airTemperatureCelsius
        );

        var windConfig = GetWindConfigFromUI();

        return new
        (
            atmosphereConfig,
            windConfig
        );
    }

    private WindConfig GetWindConfigFromUI()
    {
        var randomWindDirection = RandomWindDirectionCheckBox.IsChecked ?? false;
        var windDirection = GetWindDirection();
        var randomStrength = RandomWindStrengthCheckBox.IsChecked ?? false;
        var windStrength = WindStrengthSlider.Value;

        return new
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

        return new
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

    private void GrowForestCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        PrefillCheckBox.IsChecked = true;
        PrefillCheckBox.IsEnabled = false;

        PauseFireCheckBox.IsChecked = true;
        PauseFireCheckBox.IsEnabled = false;
    }

    private void GrowForestCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (PrefillCheckBox is not null)
        {
            PrefillCheckBox.IsEnabled = true;
            PauseFireCheckBox.IsEnabled = true;
        }
    }
}
