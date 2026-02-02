using System.Windows;
using System.Windows.Input;

namespace TheSimulation;

/// <summary>
/// Das Einstellungsfenster für die grafische Darstellung der Simulation.
/// Ermöglicht dem Benutzer, visuelle Effekte wie Partikel, Blitze und Baumformen zu konfigurieren.
/// </summary>
/// <remarks>
/// Änderungen werden erst beim Bestätigen via <see cref="ApplyGraphicsSettings"/> dauerhaft in das <see cref="GraphicsSettings"/>-Objekt übernommen.
/// </remarks>
public sealed partial class GraphicsWindow : Window
{
    /// <summary>
    /// Das aktuelle Einstellungsobjekt, das die Konfiguration hält.
    /// </summary>
    private readonly GraphicsSettings settings;

    /// <summary>
    /// Initialisiert eine neue Instanz des Grafik-Einstellungsfensters.
    /// </summary>
    /// <param name="settings">Die aktuell aktiven Grafikeinstellungen der Simulation.</param>
    public GraphicsWindow(GraphicsSettings settings)
    {
        this.settings = settings;

        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);

        // Füllt die ComboBox automatisch mit allen verfügbaren Baumformen aus dem Enum
        TreeShapeComboBox.ItemsSource = Enum.GetValues<TreeShapeType>();

        LoadGraphicsSettingsIntoUI();
    }

    /// <summary>
    /// Überträgt die Werte aus dem <see cref="settings"/>-Objekt in die entsprechenden UI-Steuerelemente (CheckBoxen, ComboBox).
    /// </summary>
    private void LoadGraphicsSettingsIntoUI()
    {
        LightningCheckBox.IsChecked = settings.ShowLightning;
        BoltFlashCheckBox.IsChecked = settings.ShowBoltFlashes;
        FireSparksCheckBox.IsChecked = settings.ShowFireParticles;
        SmokeCheckBox.IsChecked = settings.ShowSmokeParticles;
        FlameAnimationsCheckBox.IsChecked = settings.ShowFlamesOnTrees;
        BurnedTreeCheckBox.IsChecked = settings.ShowBurnedDownTrees;
        TreeShapeComboBox.SelectedItem = settings.TreeShape;
    }

    /// <summary>
    /// Verarbeitet Tastatureingaben innerhalb des Einstellungsfensters.
    /// </summary>
    /// <remarks>
    /// Die Taste <c>R</c> dient als Shortcut zum Zurücksetzen aller Einstellungen auf Standardwerte.
    /// </remarks>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.R)
        {
            ResetAllSettings_Click(sender, e);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Liest die Zustände der UI-Elemente aus, speichert sie im <see cref="settings"/>-Objekt und schließt das Fenster.
    /// </summary>
    private void ApplyGraphicsSettings(object sender, RoutedEventArgs e)
    {
        // Null-Coalescing (??) wird genutzt, um bei unbestimmten CheckBox-Zuständen Standardwerte zu setzen
        settings.ShowLightning = LightningCheckBox.IsChecked ?? true;
        settings.ShowBoltFlashes = BoltFlashCheckBox.IsChecked ?? false;
        settings.ShowFireParticles = FireSparksCheckBox.IsChecked ?? true;
        settings.ShowSmokeParticles = SmokeCheckBox.IsChecked ?? true;
        settings.ShowFlamesOnTrees = FlameAnimationsCheckBox.IsChecked ?? true;
        settings.ShowBurnedDownTrees = BurnedTreeCheckBox.IsChecked ?? false;

        if (TreeShapeComboBox.SelectedItem is TreeShapeType selectedShape)
        {
            settings.TreeShape = selectedShape;
        }

        Close();
    }

    /// <summary>
    /// Setzt alle UI-Steuerelemente auf die in <see cref="SimulationDefaultsData"/> definierten Standardwerte zurück.
    /// </summary>
    /// <remarks>
    /// Die Werte werden hier nur in der UI zurückgesetzt. Um sie dauerhaft zu speichern, muss der Benutzer dennoch auf "Apply" klicken.
    /// </remarks>
    private void ResetAllSettings_Click(object sender, RoutedEventArgs e)
    {
        // Wetter-Effekte zurücksetzen
        LightningCheckBox.IsChecked = true;
        BoltFlashCheckBox.IsChecked = false;

        // Feuer zurücksetzen
        FlameAnimationsCheckBox.IsChecked = true;

        // Partikeleffekte zurücksetzen
        FireSparksCheckBox.IsChecked = true;
        SmokeCheckBox.IsChecked = true;

        // Bäume zurücksetzen
        BurnedTreeCheckBox.IsChecked = false;
        TreeShapeComboBox.SelectedItem = SimulationDefaultsData.DefaultTreeShapeType;
    }
}
