using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

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

        simulation.SimulationTimeUpdated += text => SimulationTimeText.Text = text;
        simulation.TreeDensityUpdated += text => TreeDensityText.Text = text;
        simulation.WindStrengthUpdated += text => WindStrengthText.Text = text;
        simulation.TotalGrownTreesUpdated += text => TotalGrownTrees.Text = text;
        simulation.TotalBurnedTreesUpdated += text => TotalBurnedTrees.Text = text;

        SetSimulationSpeedUltra();

        PauseSimulation();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            PauseResume_Click(sender, e);
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

    private void PauseResume_Click(object sender, RoutedEventArgs e)
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
    {
        simulation.SetSimulationSpeed(SimulationSpeed.Slow);

        SpeedSlowButton.IsEnabled = false;

        SpeedNormalButton.IsEnabled = true;
        SpeedFastButton.IsEnabled = true;
        SpeedUltraButton.IsEnabled = true;
    }

    private void SpeedNormal_Click(object s, RoutedEventArgs e)
        => SetSimulationSpeedNormal();

    private void SetSimulationSpeedNormal()
    {
        simulation.SetSimulationSpeed(SimulationSpeed.Normal);

        SpeedNormalButton.IsEnabled = false;

        SpeedSlowButton.IsEnabled = true;
        SpeedFastButton.IsEnabled = true;
        SpeedUltraButton.IsEnabled = true;
    }

    private void SpeedFast_Click(object s, RoutedEventArgs e)
    {
        simulation.SetSimulationSpeed(SimulationSpeed.Fast);

        SpeedFastButton.IsEnabled = false;

        SpeedSlowButton.IsEnabled = true;
        SpeedNormalButton.IsEnabled = true;
        SpeedUltraButton.IsEnabled = true;
    }

    private void SpeedUltra_Click(object s, RoutedEventArgs e)
        => SetSimulationSpeedUltra();

    private void SetSimulationSpeedUltra()
    {
        simulation.SetSimulationSpeed(SimulationSpeed.Ultra);

        SpeedUltraButton.IsEnabled = false;

        SpeedSlowButton.IsEnabled = true;
        SpeedNormalButton.IsEnabled = true;
        SpeedFastButton.IsEnabled = true;
    }

    private void ShowEvaluation_Click(object sender, RoutedEventArgs e)
        => simulation.OpenEvalualtionWindow();

    // Stop timers when window is closed protection against memory leaks
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        simulation.StopOrPauseSimulation();
    }

    private void CloseOverlay_Click(object sender, RoutedEventArgs e)
        => HideOverlaySmooth();

    private void HideOverlaySmooth()
    {
        if (EditOverlay.Visibility == Visibility.Collapsed)
        {
            return;
        }

        ActivateEvaluationWindowButton();

        var fade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase
            {
                EasingMode = EasingMode.EaseOut
            }
        };

        fade.Completed += (_, __) =>
        {
            EditOverlay.Visibility = Visibility.Collapsed;
            EditOverlay.IsHitTestVisible = false;
        };

        EditOverlay.BeginAnimation(OpacityProperty, fade);
    }

    private void ActivateEvaluationWindowButton()
        => ShowEvaluationButton.IsEnabled = true;

    private void OverlayCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        => HideOverlaySmooth();

    private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        => HideOverlaySmooth();
}
