using System.Windows;
using System.Windows.Input;

namespace TheSimulation;

/// <summary>
/// Stellt das Hauptfenster der Anwendung dar, welches als Konfigurationsmenü dient.
/// Ermöglicht die Einstellung sämtlicher Simulationsparameter vor dem Start.
/// </summary>
public sealed partial class MainWindow : Window
{
    /// <summary>
    /// Die aktuell konfigurierten Grafikeinstellungen.
    /// </summary>
    private readonly GraphicsSettings graphicsSettings = new();

    /// <summary>
    /// Hilfsklasse für die Erzeugung von Zufallswerten innerhalb der Benutzeroberfläche.
    /// </summary>
    private readonly RandomHelper random = new();

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="MainWindow"/> Klasse.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);
        InitailizeWindDirectionDropdown();
    }

    /// <summary>
    /// Verarbeitet Tastatureingaben auf Fensterebene, wie z.B. das Zurücksetzen der Einstellungen.
    /// </summary>
    private void Window_KeyDown(object s, KeyEventArgs e)
    {
        if (e.Key == Key.R)
        {
            ResetAllSettings_Click(s, e);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Initialisiert die Auswahlbox für die Windrichtung mit den Werten des <see cref="WindDirection"/> Enums.
    /// </summary>
    private void InitailizeWindDirectionDropdown()
    {
        WindDirectionBox.ItemsSource = Enum.GetValues<WindDirection>();
        WindDirectionBox.SelectedItem = SimulationDefaultsData.DefaultWindDirection;
    }

    /// <summary>
    /// Startet eine neue Simulationsinstanz in einem separaten Fenster basierend auf der aktuellen UI-Konfiguration.
    /// </summary>
    private void StartSimulation_Click(object s, RoutedEventArgs e)
    {
        var config = GetSimulationConfigFromUI();
        new SimulationWindow(config, random).Show();
    }

    /// <summary>
    /// Sammelt alle Parameter aus der Benutzeroberfläche und erstellt daraus ein zentrales Konfigurationsobjekt.
    /// </summary>
    /// <returns>Ein vollständig initialisiertes <see cref="SimulationConfig"/> Objekt.</returns>
    private SimulationConfig GetSimulationConfigFromUI()
    {
        return new
        (
            CreateTreeConfig(),
            CreateFireConfig(),
            GetEnvironmentConfigFromUI(),
            GetPrefillConfigFromUI(),
            CreateVisualEffectsConfigFromUI(),
            CreateTerrainConfig()
        );
    }

    /// <summary>
    /// Erstellt die Konfiguration für die Bäume im Wald.
    /// </summary>
    private TreeConfig CreateTreeConfig()
    {
        var regrowForest = GrowForestCheckBox.IsChecked ?? true;

        return new
        (
            MaxCount: 50_000,
            ForestDensity: 0.7f,
            Size: 9,
            regrowForest
        );
    }

    /// <summary>
    /// Erstellt die Konfiguration für das Brandverhalten und Blitzschläge.
    /// </summary>
    private FireConfig CreateFireConfig()
    {
        var pauseDuringFire = PauseFireCheckBox.IsChecked ?? true;
        var fireChance = FireSpreadChanceSlider.Value;
        var lightningStrikeChance = LightningChanceSlider.Value;
        var enableLightningStrikes =
            graphicsSettings.ShowLightning && lightningStrikeChance != 0;

        return new
        (
            fireChance,
            pauseDuringFire,
            lightningStrikeChance,
            enableLightningStrikes
        );
    }

    /// <summary>
    /// Erstellt die Konfiguration für visuelle Effekte basierend auf den Grafik-Einstellungen.
    /// </summary>
    private VisualEffectsConfig CreateVisualEffectsConfigFromUI()
    {
        return new
        (
            graphicsSettings.ShowLightning,
            graphicsSettings.ShowBoltFlashes,
            graphicsSettings.ShowFireParticles,
            graphicsSettings.ShowSmokeParticles,
            graphicsSettings.ShowFlamesOnTrees,
            graphicsSettings.ShowBurnedDownTrees,
            graphicsSettings.TreeShape
        );
    }

    /// <summary>
    /// Erstellt die Konfiguration für die Geländegenerierung.
    /// </summary>
    private TerrainConfig CreateTerrainConfig()
    {
        var useTerrainGeneration = TerrainGenerationCheckBox.IsChecked ?? true;

        return new
        (
            useTerrainGeneration,
            EnableWaterBodies: false,
            EnableRocks: false
        );
    }

    /// <summary>
    /// Sammelt atmosphärische Umgebungsdaten wie Luftfeuchtigkeit und Temperatur.
    /// </summary>
    private EnvironmentConfig GetEnvironmentConfigFromUI()
    {
        var airHumidityPercentage = AirHumiditySlider.Value / 100;
        var airTemperatureCelsius = AirTemperatureSlider.Value;

        var atmosphereConfig = new AtmosphereConfig
        (
            (float)airHumidityPercentage,
            (float)airTemperatureCelsius
        );

        var windConfig = GetWindConfigFromUI();

        return new
        (
            atmosphereConfig,
            windConfig
        );
    }

    /// <summary>
    /// Sammelt die Winddaten unter Berücksichtigung von Zufallsoptionen.
    /// </summary>
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

    /// <summary>
    /// Erstellt die Konfiguration für die initiale Waldbefüllung.
    /// </summary>
    private PrefillConfig GetPrefillConfigFromUI()
    {
        var prefillDensity = PrefillDensitySlider.Value / 100;

        return new
        (
            prefillDensity >= 0,
            prefillDensity
        );
    }

    /// <summary>
    /// Ermittelt die Windrichtung, entweder fest aus der UI oder zufällig bestimmt.
    /// </summary>
    private WindDirection GetWindDirection()
    {
        if (RandomWindDirectionCheckBox.IsChecked == true)
        {
            return GetRandomWindDirection();
        }

        return GetParsedWindDirectionFromUI();
    }

    /// <summary>
    /// Extrahiert die gewählte Windrichtung aus der Auswahlbox.
    /// </summary>
    private WindDirection GetParsedWindDirectionFromUI()
    {
        return WindDirectionBox.SelectedItem is WindDirection direction
            ? direction
            : SimulationDefaultsData.DefaultWindDirection;
    }

    /// <summary>
    /// Erzeugt eine zufällige Windrichtung aus allen verfügbaren Enum-Werten.
    /// </summary>
    private WindDirection GetRandomWindDirection()
    {
        var values = Enum.GetValues<WindDirection>();
        var randomIndex = random.NextInt(0, values.Length);
        return values[randomIndex];
    }

    /// <summary>
    /// Reagiert auf Änderungen an der Checkbox für zufällige Windrichtung und aktiviert/deaktiviert die manuelle Auswahl.
    /// </summary>
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

    /// <summary>
    /// Öffnet das Fenster für detaillierte Grafikeinstellungen als modalen Dialog.
    /// </summary>
    private void OpenGraphicsSettingsWindow(object s, RoutedEventArgs e)
    {
        var graphicsSettingsWindow = new GraphicsWindow(graphicsSettings);
        graphicsSettingsWindow.ShowDialog();
    }

    /// <summary>
    /// Setzt die UI-Elemente in einen konsistenten Zustand, wenn das Nachwachsen deaktiviert wird.
    /// </summary>
    private void GrowForestCheckBox_Unchecked(object s, RoutedEventArgs e)
    {
        PauseFireCheckBox.IsChecked = true;
        PauseFireCheckBox.IsEnabled = false;
    }

    /// <summary>
    /// Steuert die Verfügbarkeit der Feuer-Pause-Option basierend auf dem Waldwachstum.
    /// Deaktiviert die Option, wenn kein Wald nachwachsen kann, um einen dauerhaften Stillstand zu vermeiden.
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

    /// <summary>
    /// Setzt alle Schieberegler und Optionen im Menü auf die Standardwerte zurück.
    /// </summary>
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

    /// <summary>
    /// Aktualisiert die Textanzeige für die Windstärke inklusive Umrechnung in die Beaufort-Skala bei Schieberegler-Änderung.
    /// </summary>
    private void WindStrengthSlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    {
        var windStrength = WindStrengthSlider.Value;
        var windStrengthReadablePercent = windStrength * 100;
        var beaufortScale = (int)WindMapper.ConvertWindPercentStrenghToBeaufort(windStrength);

        WindStrengthText.Text =
            $"Wind Strengh: {windStrengthReadablePercent:F0}% ({beaufortScale} Bft)";
    }
}
