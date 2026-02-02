using System.Windows;
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

    private void Window_KeyDown(object s, KeyEventArgs e)
    {
        if (e.Key == Key.R)
        {
            ResetAllSettings_Click(s, e);
            e.Handled = true;
        }
    }

    private void InitailizeWindDirectionDropdown()
    {
        WindDirectionBox.ItemsSource = Enum.GetValues<WindDirection>();
        WindDirectionBox.SelectedItem = SimulationDefaultsData.DefaultWindDirection;
    }

    private void StartSimulation_Click(object s, RoutedEventArgs e)
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
        var fireChance = FireSpreadChanceSlider.Value; // (Additional fire spread chance)
        var lightningStrikeChance = LightningChanceSlider.Value;
        var enableLightningStrikes =
            graphicsSettings.ShowLightning && lightningStrikeChance != 0;

        var fireConfig = new FireConfig
        (
            fireChance,
            pauseDuringFire,
            lightningStrikeChance,
            enableLightningStrikes
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
        return WindDirectionBox.SelectedItem is WindDirection direction
            ? direction
            : SimulationDefaultsData.DefaultWindDirection;
    }

    private WindDirection GetRandomWindDirection()
    {
        var values = Enum.GetValues<WindDirection>();
        var randomIndex = random.NextInt(0, values.Length);
        return values[randomIndex];
    }

    private void RandomWindDirectionCheckBox_Changed(object s, RoutedEventArgs e)
    {
        var isRandom = RandomWindDirectionCheckBox.IsChecked ?? false;
        WindDirectionBox.IsEnabled = !isRandom;

        if (!isRandom)
        {
            return;
        }

        WindDirectionBox.SelectedItem = GetRandomWindDirection();
    }

    private void OpenGraphicsSettingsWindow(object s, RoutedEventArgs e)
    {
        var graphicsSettingsWindow = new GraphicsWindow(graphicsSettings);
        graphicsSettingsWindow.ShowDialog();
    }

    private void GrowForestCheckBox_Unchecked(object s, RoutedEventArgs e)
    {
        PauseFireCheckBox.IsChecked = true;
        PauseFireCheckBox.IsEnabled = false;
    }

    /// <summary>
    /// Steuert die Verfügbarkeit der Feuer-Pause-Option basierend auf dem Waldwachstum.
    /// </summary>
    private void GrowForestCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (PauseFireCheckBox is null)
        {
            return;
        }

        var isForestRegrowEnabled = GrowForestCheckBox.IsChecked ?? true;

        // Deaktiviere die Pause-Option, wenn kein Wald nachwachsen kann
        PauseFireCheckBox.IsEnabled = isForestRegrowEnabled;

        // Wenn Waldwachstum aus ist, setzen wir die Pause automatisch auf "An"
        if (!isForestRegrowEnabled)
        {
            PauseFireCheckBox.IsChecked = true;
        }
    }

    private void ResetAllSettings_Click(object s, RoutedEventArgs e)
    {
        // Terrain zurücksetzen
        TerrainGenerationCheckBox.IsChecked = true;

        // Bäume zurücksetzen
        GrowForestCheckBox.IsChecked = true;
        PrefillDensitySlider.Value = 80;
        LightningChanceSlider.Value = 15;

        // Feuer zurücksetzen
        PauseFireCheckBox.IsChecked = true;
        FireSpreadChanceSlider.Value = 40;

        // Umwelt zurücksetzen
        AirHumiditySlider.Value = 50;
        AirTemperatureSlider.Value = 30;
        RandomWindDirectionCheckBox.IsChecked = false;
        WindDirectionBox.SelectedItem = SimulationDefaultsData.DefaultWindDirection;
        RandomWindStrengthCheckBox.IsChecked = false;
        WindStrengthSlider.Value = 0.75;
    }

    private void WindStrengthSlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    {
        var windStrength = WindStrengthSlider.Value;

        var windStrengthReadablePercent = windStrength * 100;
        var beaufortScale = (int)WindMapper.ConvertWindPercentStrenghToBeaufort(windStrength);

        WindStrengthText.Text =
            $"Wind Strengh: {windStrengthReadablePercent:F0}% ({beaufortScale} Bft)";
    }
}
