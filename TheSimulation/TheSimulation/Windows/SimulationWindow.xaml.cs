using System.Windows;
using System.Windows.Input;

namespace TheSimulation;

/// <summary>
/// Das Hauptfenster der Simulation. Verwaltet die visuelle Darstellung des Waldes, 
/// die Benutzereingaben (Tastatur/Maus) und die Steuerung der Simulationsgeschwindigkeit.
/// </summary>
public sealed partial class SimulationWindow : Window
{
    /// <summary>
    /// Der Kern-Controller, der die Brandlogik und das Zellen-Update verwaltet.
    /// </summary>
    private readonly ForestFireSimulation simulation;

    /// <summary>
    /// Initialisiert eine neue Instanz des Simulationsfensters.
    /// </summary>
    /// <param name="simulationConfig">Die Starteinstellungen für Waldgröße, Wetter etc.</param>
    /// <param name="random">Ein zentraler Zufallsgenerator für deterministische oder zufällige Abläufe.</param>
    public SimulationWindow(SimulationConfig simulationConfig, RandomHelper random)
    {
        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);

        var startSpeed = SimulationDefaultsData.DefaultSimulationSpeed;

        // Initialisierung der Logik-Engine mit Anbindung an das UI-Canvas
        simulation = new ForestFireSimulation(simulationConfig, random, ForestCanvas, startSpeed);

        // DataContext-Bindung für Live-Statistiken (z. B. Anzahl brennender Bäume) im UI
        DataContext = simulation.SimulationLiveStats;

        UpdateSpeedUI(startSpeed);

        // Simulation startet standardmäßig im Pausenmodus, damit der Nutzer sich orientieren kann
        PauseSimulation();
    }

    /// <summary>
    /// Verarbeitet globale Tastatureingaben für das Fenster.
    /// </summary>
    private void Window_KeyDown(object s, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Space: // Pause/Fortsetzen Umschalter
                PauseResume_Click(s, e);
                e.Handled = true;
                break;

            case Key.Escape: // Overlay schließen
                HideOverlaySmooth();
                e.Handled = true;
                break;
        }
    }

    /// <summary>
    /// Versetzt die Simulation in den Pausenzustand und aktualisiert die Schaltflächensymbole.
    /// </summary>
    private void PauseSimulation()
    {
        simulation.StopOrPauseSimulation();
        PauseResumeButton.Content = "▶";
    }

    /// <summary>
    /// Schaltet zwischen Pause und laufender Simulation um.
    /// </summary>
    private void PauseResume_Click(object s, RoutedEventArgs e)
    {
        if (simulation.IsPaused)
        {
            simulation.StartOrResumeSimulation();
            PauseResumeButton.Content = "❚❚";
            HideOverlaySmooth();
            return;
        }

        PauseSimulation();
    }

    /// <summary>
    /// Eventhandler für die Geschwindigkeitssteuerung. Liest den Wert aus dem <see cref="FrameworkElement.Tag"/> des Buttons.
    /// </summary>
    private void SpeedButton_Click(object s, RoutedEventArgs e)
    {
        if (s is FrameworkElement element && element.Tag is string speedString)
        {
            if (Enum.TryParse<SimulationSpeed>(speedString, out var speed))
            {
                UpdateSpeedUI(speed);
            }
        }
    }

    /// <summary>
    /// Aktualisiert die Simulationsgeschwindigkeit und passt die UI-Zustände der Buttons an.
    /// </summary>
    /// <param name="speed">Die neu zu setzende Geschwindigkeit.</param>
    private void UpdateSpeedUI(SimulationSpeed speed)
    {
        simulation.SetSimulationSpeed(speed);

        // Deaktiviert den Button der aktuellen Geschwindigkeit, um aktiven Zustand zu visualisieren
        SpeedSlowButton.IsEnabled = speed is not SimulationSpeed.Slow;
        SpeedNormalButton.IsEnabled = speed is not SimulationSpeed.Normal;
        SpeedFastButton.IsEnabled = speed is not SimulationSpeed.Fast;
        SpeedUltraButton.IsEnabled = speed is not SimulationSpeed.Ultra;
    }

    /// <summary>
    /// Öffnet das Fenster für die detaillierte statistische Auswertung.
    /// </summary>
    private void ShowEvaluation_Click(object s, RoutedEventArgs e)
        => simulation.OpenEvalualtionWindow();

    /// <summary>
    /// Wird beim Schließen des Fensters aufgerufen. 
    /// Stellt sicher, dass Ressourcen freigegeben und Simulations-Timer gestoppt werden.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        simulation?.Dispose();
    }

    /// <summary>
    /// Schließt das Bearbeitungs- oder Infofeld mit einer sanften Animation.
    /// </summary>
    private void HideOverlaySmooth()
    {
        if (EditOverlay.Visibility == Visibility.Collapsed)
        {
            return;
        }

        ActivateEvaluationWindowButton();

        // Nutzt den Hilfsanimator für ein weiches Ausblenden
        UIAnimationHelper.FadeOut(EditOverlay, SetEditOverlayCollapsedValues);
    }

    /// <summary>
    /// Callback nach Abschluss der Ausblend-Animation. 
    /// Setzt die Sichtbarkeit hart auf Collapsed, um Layout-Berechnungen zu sparen.
    /// </summary>
    private void SetEditOverlayCollapsedValues()
    {
        EditOverlay.Visibility = Visibility.Collapsed;
        EditOverlay.IsHitTestVisible = false;
    }

    /// <summary>
    /// Aktiviert die Schaltfläche für die Auswertung (sobald die erste Simulation interagiert wurde).
    /// </summary>
    private void ActivateEvaluationWindowButton()
        => ShowEvaluationButton.IsEnabled = true;

    /// <summary>
    /// Ermöglicht das Schließen des Overlays durch Klicken auf den Hintergrund.
    /// </summary>
    private void OverlayCanvas_MouseDown(object s, MouseButtonEventArgs e)
        => HideOverlaySmooth();

    private void CloseOverlay_Click(object sender, RoutedEventArgs e)
        => HideOverlaySmooth();
}
