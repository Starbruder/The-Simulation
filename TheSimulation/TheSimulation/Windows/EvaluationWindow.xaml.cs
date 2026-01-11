using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
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
        var model = new PlotModel
        {
            Title = "Grown / Burned Trees"
        };

        model.Legends.Add(new Legend
        {
            IsLegendVisible = true,
            LegendPlacement = LegendPlacement.Inside,
            LegendPosition = LegendPosition.TopLeft,
        });


        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Time (s)"
        });

        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Trees"
        });

        var grown = new LineSeries
        {
            Title = "Total Grown Trees",
            Color = OxyColors.Green
        };

        var burned = new LineSeries
        {
            Title = "Total Burned Trees",
            Color = OxyColors.Red
        };

        foreach (var history in data.History)
        {
            var t = history.Time.TotalSeconds;
            grown.Points.Add(new(t, history.Grown));
            burned.Points.Add(new(t, history.Burned));
        }

        model.Series.Add(grown);
        model.Series.Add(burned);

        GrownBurnedPlot.Model = model;
    }

    private void DrawActiveTreesChart()
    {
        var model = new PlotModel
        {
            Title = "Active Trees"
        };

        model.Legends.Add(new Legend
        {
            IsLegendVisible = true,
            LegendPlacement = LegendPlacement.Inside,
            LegendPosition = LegendPosition.TopLeft
        });

        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Time (s)"
        });

        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Trees",
            //Maximum = data.MaxTreesPossible // direkt vorskaliert
        });

        var active = new LineSeries
        {
            Title = "Active Trees",
            Color = OxyColors.DarkGreen
        };

        foreach (var history in data.History)
        {
            var activeTrees = (int)history.Grown - (int)history.Burned;
            active.Points.Add(
                new(history.Time.TotalSeconds, activeTrees)
            );
        }

        model.Series.Add(active);

        ActiveTreesPlot.Model = model;
    }
}
