using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TheSimulation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly GraphicsSettings graphicsSettings = new();
    private readonly RandomHelper random = new();

    public MainWindow()
    {
        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);
        InitailizeWindDirectionDropdown();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.R)
        {
            ResetAllSettings_Click(sender, e);
            e.Handled = true;
        }
    }

    private void InitailizeWindDirectionDropdown()
    {
        var enumtypes = Enum.GetValues<WindDirection>();
        foreach (var enumtype in enumtypes)
        {
            var windDirectionItem = new ComboBoxItem().Content = enumtype;
            WindDirectionBox.Items.Add(windDirectionItem);
        }
    }

    private void StartSimulation_Click(object sender, RoutedEventArgs e)
    {
        var config = GetSimulationConfigFromUI();
        new SimulationWindow(config, random).Show();
    }

    private SimulationConfig GetSimulationConfigFromUI()
    {
        var regrowForest = GrowForestCheckBox.IsChecked ?? true;

        var treeConfig = new TreeConfig
        (
            MaxCount: 50_000,
            ForestDensity: 0.7f,
            Size: 9,
            AllowRegrowForest: regrowForest
        );

        var pauseDuringFire = PauseFireCheckBox.IsChecked ?? true;
        var fireChance = FireSpreadChanceSlider.Value; // (Additional fire spread chance percent)

        var fireConfig = new FireConfig
        (
            fireChance,
            pauseDuringFire
        );

        var environmentConfig = GetEnvironmentConfigFromUI();

        var prefillConfig = GetPrefillConfigFromUI();

        var effectsConfig = new VisualEffectsConfig
        (
            graphicsSettings.ShowLightning,
            graphicsSettings.ShowBoltFlashes,
            graphicsSettings.ShowFireParticles,
            graphicsSettings.ShowSmokeParticles,
            graphicsSettings.ShowFlamesOnTrees,
            graphicsSettings.ShowBurnedDownTrees, // TODO : Nach der Zeit verbrannter Baum wieder verschwinden lassen
            graphicsSettings.TreeShape
        );

        var useTerrainGeneration = TerrainGenerationCheckBox.IsChecked ?? true;

        var terrainConfig = new TerrainConfig
        (
            UseTerrainGeneration: useTerrainGeneration,
            EnableWaterBodies: false,
            EnableRocks: false
        );

        return new
        (
            treeConfig,
            fireConfig,
            environmentConfig,
            prefillConfig,
            effectsConfig,
            terrainConfig
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
        var prefillDensity = PrefillDensitySlider.Value / 100; // 0..1

        return new
        (
            prefillDensity >= 0,
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
        var selectedComboBoxItem = WindDirectionBox.SelectedItem;

        var selectedString = selectedComboBoxItem.ToString();

        return Enum.TryParse<WindDirection>(selectedString, out var windDir)
            ? windDir
            : WindDirection.North;
    }

    private WindDirection GetRandomWindDirection()
    {
        var values = Enum.GetValues(typeof(WindDirection));
        var randomIndex = random.NextInt(0, values.Length);
        return (WindDirection)values.GetValue(randomIndex);
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
        foreach (var item in WindDirectionBox.Items)
        {
            if (Enum.TryParse<WindDirection>(item.ToString(), out var dir)
                && dir == randomDirection)
            {
                WindDirectionBox.SelectedItem = item;
                break;
            }
        }
    }

    private void OpenGraphicsSettingsWindow(object sender, RoutedEventArgs e)
    {
        var graphicsSettingsWindow = new GraphicsWindow(graphicsSettings);
        graphicsSettingsWindow.ShowDialog();
    }

    private void GrowForestCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        PauseFireCheckBox.IsChecked = true;
        PauseFireCheckBox.IsEnabled = false;
    }

    private void GrowForestCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (PauseFireCheckBox is not null)
        {
            PauseFireCheckBox.IsEnabled = true;
        }
    }

    private void ResetAllSettings_Click(object sender, RoutedEventArgs e) => SetDefaultSettings();

    private void SetDefaultSettings()
    {
        // Terrain zurücksetzen
        TerrainGenerationCheckBox.IsChecked = true;

        // Bäume zurücksetzen
        GrowForestCheckBox.IsChecked = true;
        PrefillDensitySlider.Value = 80;

        // Feuer zurücksetzen
        PauseFireCheckBox.IsChecked = true;
        FireSpreadChanceSlider.Value = 40;

        // Umwelt zurücksetzen
        AirHumiditySlider.Value = 50;
        AirTemperatureSlider.Value = 30;
        RandomWindDirectionCheckBox.IsChecked = false;
        WindDirectionBox.SelectedIndex = 0;
        RandomWindStrengthCheckBox.IsChecked = false;
        WindStrengthSlider.Value = 0.75;
    }

    private void WindStrengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var windStrength = WindStrengthSlider.Value;

        var windStrengthReadablePercent = windStrength * 100;
        var beaufortScale = (int)WindMapper.ConvertWindPercentStrenghToBeaufort(windStrength);

        WindStrengthText.Text =
            $"Wind Strengh: {windStrengthReadablePercent:F0}% ({beaufortScale} Bft)";
    }

    /// <summary>
    /// Prüft, dass mindestens eine Option aktiviert bleibt.
    /// Wird ausgelöst, wenn GrowForestCheckBox geändert wird.
    /// </summary>
    private void GrowForestCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        // Optional: PauseFireCheckBox nur aktiv, wenn GrowForest an ist
        if (PauseFireCheckBox is not null)
        {
            PauseFireCheckBox.IsEnabled = GrowForestCheckBox.IsChecked ?? true;
        }
    }
}
