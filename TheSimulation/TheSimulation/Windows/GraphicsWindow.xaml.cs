using System.Windows;
using System.Windows.Input;

namespace TheSimulation;

/// <summary>
/// Interaktionslogik für das Grafikeinstellungen-Fenster.
/// Ermöglicht dem Benutzer das Anpassen visueller Parameter der Simulation.
/// </summary>
public sealed partial class GraphicsWindow : Window
{
    private readonly GraphicsViewModel viewModel;
    private readonly GraphicsSettings originalSettings;

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="GraphicsWindow"/> Klasse.
    /// </summary>
    /// <param name="settings">Die aktuellen Grafikeinstellungen, die bearbeitet werden sollen.</param>
    public GraphicsWindow(GraphicsSettings settings)
    {
        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);

        originalSettings = settings;
        viewModel = new(settings);

        // Das Bindeglied zwischen XAML und Code
        DataContext = viewModel;
    }

    /// <summary>
    /// Behandelt Tastatureingaben innerhalb des Fensters.
    /// Drücken der Taste 'R' setzt das ViewModel auf Standardwerte zurück.
    /// </summary>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.R)
        {
            viewModel.Reset();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Übernimmt die im ViewModel geänderten Arbeitseinstellungen in das Original-Objekt 
    /// und schließt das Fenster mit einem positiven Ergebnis.
    /// </summary>
    private void ApplyGraphicsSettings(object sender, RoutedEventArgs e)
    {
        // Hier übertragen wir die Werte der Arbeitskopie zurück in das Original
        var updated = viewModel.WorkingSettings;

        originalSettings.ShowLightning = updated.ShowLightning;
        originalSettings.ShowBoltFlashes = updated.ShowBoltFlashes;
        originalSettings.ShowFireParticles = updated.ShowFireParticles;
        originalSettings.ShowSmokeParticles = updated.ShowSmokeParticles;
        originalSettings.ShowFlamesOnTrees = updated.ShowFlamesOnTrees;
        originalSettings.ShowBurnedDownTrees = updated.ShowBurnedDownTrees;
        originalSettings.TreeShape = updated.TreeShape;

        DialogResult = true; // Signalisiert Erfolg
        Close();
    }

    /// <summary>
    /// Ereignishandler für den Reset-Button, um alle Einstellungen 
    /// im ViewModel auf die Standardwerte zurückzusetzen.
    /// </summary>
    private void ResetAllSettings_Click(object sender, RoutedEventArgs e)
        => viewModel.Reset();
}
