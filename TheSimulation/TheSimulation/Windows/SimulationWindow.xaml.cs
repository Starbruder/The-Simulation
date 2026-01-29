using System.Windows;
using System.Windows.Input;

namespace TheSimulation;

/// <summary>
/// Interaktionslogik für SimulationWindow.xaml
/// </summary>
public sealed partial class SimulationWindow : Window
{
    private readonly ForestFireSimulation simulation;

    public SimulationWindow(SimulationConfig simulationConfig, RandomHelper random)
    {
        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);

        simulation = new
            (simulationConfig, random, ForestCanvas, SimulationSpeed.Ultra);

        DataContext = simulation.SimulationLiveStats;

        UpdateSpeedUI(SimulationSpeed.Ultra);

        PauseSimulation();
    }

    private void Window_KeyDown(object s, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            PauseResume_Click(s, e);
            e.Handled = true;
        }

        if (e.Key == Key.Escape)
        {
            HideOverlaySmooth();
            e.Handled = true;
        }
    }

    private void PauseSimulation()
    {
        simulation.StopOrPauseSimulation();
        PauseResumeButton.Content = "▶";
    }

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

    private void SpeedSlow_Click(object s, RoutedEventArgs e)
        => UpdateSpeedUI(SimulationSpeed.Slow);

    private void SpeedNormal_Click(object s, RoutedEventArgs e)
        => UpdateSpeedUI(SimulationSpeed.Normal);

    private void SpeedFast_Click(object s, RoutedEventArgs e)
        => UpdateSpeedUI(SimulationSpeed.Fast);

    private void SpeedUltra_Click(object s, RoutedEventArgs e)
        => UpdateSpeedUI(SimulationSpeed.Ultra);

    private void UpdateSpeedUI(SimulationSpeed speed)
    {
        simulation.SetSimulationSpeed(speed);

        // Check which button to disable (showing current speed as mimicing a clicked button)
        SpeedSlowButton.IsEnabled = speed != SimulationSpeed.Slow;
        SpeedNormalButton.IsEnabled = speed != SimulationSpeed.Normal;
        SpeedFastButton.IsEnabled = speed != SimulationSpeed.Fast;
        SpeedUltraButton.IsEnabled = speed != SimulationSpeed.Ultra;
    }

    private void ShowEvaluation_Click(object s, RoutedEventArgs e)
        => simulation.OpenEvalualtionWindow();

    // Stop timers when window is closed protection against memory leaks
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        simulation.StopOrPauseSimulation();
    }

    private void CloseOverlay_Click(object s, RoutedEventArgs e)
        => HideOverlaySmooth();

    private void HideOverlaySmooth()
    {
        if (EditOverlay.Visibility == Visibility.Collapsed)
        {
            return;
        }

        ActivateEvaluationWindowButton();

        UIAnimationHelper.FadeOut(EditOverlay, SetEditOverlayCollapsedValues);
    }

    private void SetEditOverlayCollapsedValues()
    {
        EditOverlay.Visibility = Visibility.Collapsed;
        EditOverlay.IsHitTestVisible = false;
    }

    private void ActivateEvaluationWindowButton()
        => ShowEvaluationButton.IsEnabled = true;

    private void OverlayCanvas_MouseDown(object s, MouseButtonEventArgs e)
        => HideOverlaySmooth();
}
