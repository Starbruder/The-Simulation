using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace TheSimulation;

public static class ChartVisualizer
{
    public static PlotModel CreateLineChart(string title, string xAxisTitle, string yAxisTitle)
    {
        var model = new PlotModel
        {
            Title = title
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
            Title = xAxisTitle
        });

        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = yAxisTitle
        });

        return model;
    }

    public static LineSeries CreateLineSeries(string title, OxyColor color, LineStyle lineStyle = LineStyle.Solid, double thickness = 1)
    {
        return new LineSeries
        {
            Title = title,
            Color = color,
            LineStyle = lineStyle,
            StrokeThickness = thickness
        };
    }

    /// <summary>
    /// Erstellt eine horizontale Linie für den Durchschnitt einer Serie.
    /// </summary>
    /// <param name="history">Liste der History-Einträge.</param>
    /// <param name="title">Titel der Linie.</param>
    /// <param name="color">Farbe der Linie.</param>
    /// <returns>LineSeries für die Durchschnittslinie.</returns>
    public static LineSeries CreateAverageLine
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
