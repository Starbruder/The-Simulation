using System.Windows;

namespace TheSimulation;

/// <summary>
/// Interaktionslogik für EvaluationWindow.xaml
/// </summary>
public sealed partial class EvaluationWindow : Window
{
    public EvaluationWindow(string TotalGrownTrees, string TotalBurnedTrees, string TreeDensityText, string SimulationTimeText)
    {
        InitializeComponent();

        EvalTotalGrown.Text = $"Total Grown: {TotalGrownTrees}";
        EvalTotalBurned.Text = $"Total Burned: {TotalBurnedTrees}";
        EvalTreeDensity.Text = $"Tree Density: {TreeDensityText}";
        EvalRuntime.Text = $"Runtime: {SimulationTimeText}";
    }
}
