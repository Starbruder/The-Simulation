using System.Windows;
using System.Windows.Input;

namespace TheSimulation;

/// <summary>
/// Stellt das Hauptfenster der Anwendung dar, welches als Konfigurationsmenü dient.
/// Ermöglicht die Einstellung sämtlicher Simulationsparameter vor dem Start.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly ConfigurationViewModel viewModel = new();
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
        InitializeWindDirectionDropdown();
        DataContext = viewModel;
    }

    /// <summary>
    /// Initialisiert die Auswahlbox für die Windrichtung mit den Werten des <see cref="WindDirection"/> Enums.
    /// </summary>
    private void InitializeWindDirectionDropdown()
    {
        viewModel.SelectedWindDirection = SimulationDefaultsData.DefaultWindDirection;
    }

    /// <summary>
    /// Verarbeitet Tastatureingaben auf Fensterebene, wie z.B. das Zurücksetzen der Einstellungen.
    /// </summary>
    private void Window_KeyDown(object s, KeyEventArgs e)
    {
        if (e.Key == Key.R)
        {
            viewModel.ResetToDefaults();
            e.Handled = true;
        }
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
        return new
        (
            MaxCount: 50_000,
            ForestDensity: 0.7f,
            Size: 9,
            viewModel.GrowForest // Nutzt ViewModel Property
        );
    }

    /// <summary>
    /// Erstellt die Konfiguration für das Brandverhalten und Blitzschläge.
    /// </summary>
    private FireConfig CreateFireConfig()
    {
        return new
        (
            viewModel.FireSpreadChance,
            viewModel.PauseGrowingDuringFire,
            viewModel.LightningChance,
            graphicsSettings.ShowLightning && viewModel.LightningChance != 0
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
        return new
        (
            viewModel.UseTerrainGeneration,
            EnableWaterBodies: false,
            EnableRocks: false
        );
    }

    /// <summary>
    /// Sammelt atmosphärische Umgebungsdaten wie Luftfeuchtigkeit und Temperatur.
    /// </summary>
    private EnvironmentConfig GetEnvironmentConfigFromUI()
    {
        var airHumidityPercentage = viewModel.AirHumidity / 100;
        var airTemperatureCelsius = viewModel.AirTemperature;

        var atmosphereConfig = new AtmosphereConfig
        (
            (float)airHumidityPercentage,
            (float)airTemperatureCelsius
        );

        return new
        (
            atmosphereConfig,
            GetWindConfigFromUI()
        );
    }

    /// <summary>
    /// Sammelt die Winddaten unter Berücksichtigung von Zufallsoptionen.
    /// </summary>
    private WindConfig GetWindConfigFromUI()
    {
        return new
        (
            viewModel.RandomWindDirection,
            viewModel.SelectedWindDirection,
            viewModel.RandomWindStrength,
            viewModel.WindStrength
        );
    }

    /// <summary>
    /// Erstellt die Konfiguration für die initiale Waldbefüllung.
    /// </summary>
    private PrefillConfig GetPrefillConfigFromUI()
    {
        var prefillDensity = viewModel.PrefillDensity / 100;

        return new
        (
            viewModel.PrefillDensity >= 0,
            prefillDensity
        );
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
        var isStaticWindDirection = !viewModel.RandomWindDirection;
        viewModel.IsWindDirectionBoxEnabled = isStaticWindDirection;

        if (isStaticWindDirection)
        {
            return;
        }

        viewModel.SelectedWindDirection = GetRandomWindDirection();
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
        viewModel.IsPauseFireEnabled = false;
        viewModel.PauseGrowingDuringFire = true;
    }

    /// <summary>
    /// Steuert die Verfügbarkeit der Feuer-Pause-Option basierend auf dem Waldwachstum.
    /// Deaktiviert die Option, wenn kein Wald nachwachsen kann.
    /// </summary>
    private void GrowForestCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        var isForestRegrowEnabled = viewModel.GrowForest;

        viewModel.IsPauseFireEnabled = isForestRegrowEnabled;

        if (!isForestRegrowEnabled)
        {
            // Setze den Wert auf false (Haken entfernen)
            viewModel.PauseGrowingDuringFire = false;
        }
    }

    /// <summary>
    /// Setzt alle Schieberegler und Optionen im Menü auf die Standardwerte zurück.
    /// </summary>
    private void ResetAllSettings_Click(object s, RoutedEventArgs e)
        => viewModel.ResetToDefaults();

    /// <summary>
    /// Aktualisiert die Textanzeige für die Windstärke inklusive Umrechnung in die Beaufort-Skala bei Schieberegler-Änderung.
    /// </summary>
    private void WindStrengthSlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    {
        if (viewModel is not null)
        {
            viewModel.WindStrength = e.NewValue;
        }
    }
}
