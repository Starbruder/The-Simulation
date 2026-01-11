using OxyPlot;
using System.Windows;

namespace TheSimulation;

/// <summary>
/// Interaktionslogik für EvaluationWindow.xaml
/// </summary>
public sealed partial class EvaluationWindow : Window
{
    private readonly EvaluationData data;

    public EvaluationWindow(EvaluationData data)
    {
        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);
        this.data = data;

        EvalRuntime.Text = $"Runtime: {data.Runtime:hh\\:mm\\:ss}";

        Loaded += (_, _) =>
        {
            DrawSimulationChart();
            DrawActiveTreesChart();
        };
    }

    private void DrawSimulationChart()
    {
        var model = ChartVisualizer.CreateLineChart("Grown / Burned Trees", "Time (s)", "Trees");

        var grown =
            ChartVisualizer.CreateLineSeries("Total Grown Trees", OxyColors.Green);
        var burned =
            ChartVisualizer.CreateLineSeries("Total Burned Trees", OxyColors.Red);

        foreach (var historySnapshot in data.History)
        {
            var t = historySnapshot.Time.TotalSeconds;
            grown.Points.Add(new(t, historySnapshot.Grown));
            burned.Points.Add(new(t, historySnapshot.Burned));
        }

        model.Series.Add(grown);
        model.Series.Add(burned);

        GrownBurnedPlot.Model = model;
    }

    private void DrawActiveTreesChart()
    {
        var model =
            ChartVisualizer.CreateLineChart("Active Trees", "Time (s)", "Trees");

        var active =
            ChartVisualizer.CreateLineSeries("Active Trees", OxyColors.DarkGreen);

        foreach (var h in data.History)
        {
            var activeCount = (int)h.Grown - (int)h.Burned;
            active.Points.Add(new(h.Time.TotalSeconds, activeCount));
        }

        model.Series.Add(active);

        model.Series.Add(
            ChartVisualizer.CreateAverageLine(
                data.History, "Average Active Trees", OxyColors.Orange)
        );

        ActiveTreesPlot.Model = model;
    }
}
