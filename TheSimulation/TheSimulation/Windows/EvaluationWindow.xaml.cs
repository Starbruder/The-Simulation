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

        var sumActive = 0f;

        foreach (var history in data.History)
        {
            var activeTrees = (int)history.Grown - (int)history.Burned;
            sumActive += activeTrees;
            active.Points.Add(
                new(history.Time.TotalSeconds, activeTrees)
            );
        }

        model.Series.Add(active);

        model.Series.Add(
            CreateAverageLine(data.History, "Average Active Trees", OxyColors.Orange)
        );

        ActiveTreesPlot.Model = model;
    }

    /// <summary>
    /// Erstellt eine horizontale Linie für den Durchschnitt einer Serie.
    /// </summary>
    /// <param name="history">Liste der History-Einträge.</param>
    /// <param name="title">Titel der Linie.</param>
    /// <param name="color">Farbe der Linie.</param>
    /// <returns>LineSeries für die Durchschnittslinie.</returns>
    private static LineSeries CreateAverageLine
        (List<(TimeSpan Time, uint Grown, uint Burned)> history, string title, OxyColor color)
    {
        var sum = 0f;
        foreach (var h in history)
        {
            sum += (int)h.Grown - (int)h.Burned;
        }
        var average = sum / history.Count;

        var line = new LineSeries
        {
            Title = title,
            Color = color,
            StrokeThickness = 2,
            LineStyle = LineStyle.Dash
        };

        var startTime = history[0].Time.TotalSeconds;
        var endTime = history[^1].Time.TotalSeconds;

        line.Points.Add(new(startTime, average));
        line.Points.Add(new(endTime, average));

        return line;
    }
}
