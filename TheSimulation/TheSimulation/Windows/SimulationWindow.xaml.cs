using System.Windows;

namespace TheSimulation;

/// <summary>
/// Interaktionslogik für SimulationWindow.xaml
/// </summary>
public sealed partial class SimulationWindow : Window
{
    private readonly Simulation simulation;

    public SimulationWindow(SimulationConfig simulationConfig)
    {
        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);

        simulation = new Simulation(simulationConfig, ForestCanvas)
        {
            UpdateSimulationTimeText = text => SimulationTimeText.Text = text,
            UpdateTreeDensityText = text => TreeDensityText.Text = text,
            UpdateWindStrengthText = text => WindStrengthText.Text = text,
            UpdateTotalGrownTreesText = text => TotalGrownTrees.Text = text,
            UpdateTotalBurnedTreesText = text => TotalBurnedTrees.Text = text
        };

        SetSimulationSpeedNormal();
    }

    private void PauseResume_Click(object sender, RoutedEventArgs e)
    {
        if (simulation.isPaused)
        {
            simulation.StartOrResumeSimulation();
            PauseResumeButton.Content = "❚❚";
            MessageBox.Show("Simulation resumed.");
            return;
        }

        simulation.StopOrPauseSimulation();
        PauseResumeButton.Content = "▶";
        MessageBox.Show("Simulation paused.");
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
}
