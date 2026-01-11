using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace TheSimulation;

/// <summary>
/// Statische Hilfsklasse zum Erstellen und Visualisieren von OxyPlot-Diagrammen.
/// Kapselt die Erstellung von PlotModels, LineSeries und Durchschnittslinien.
/// </summary>
public static class ChartVisualizer
{
	/// <summary>
	/// Erstellt ein neues Liniendiagramm mit Achsen und einer Legende.
	/// </summary>
	/// <param name="title">Titel des Diagramms.</param>
	/// <param name="xAxisTitle">Titel der X-Achse.</param>
	/// <param name="yAxisTitle">Titel der Y-Achse.</param>
	/// <returns>Ein neues <see cref="PlotModel"/> mit konfigurierten Achsen und Legende.</returns>
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

	/// <summary>
	/// Erstellt eine <see cref="LineSeries"/> für ein Diagramm.
	/// </summary>
	/// <param name="title">Titel der Linie, der auch in der Legende erscheint.</param>
	/// <param name="color">Farbe der Linie.</param>
	/// <param name="lineStyle">Linienstil (z.B. Solid, Dash).</param>
	/// <param name="thickness">Dicke der Linie.</param>
	/// <returns>Eine konfigurierte <see cref="LineSeries"/>.</returns>
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
	/// <param name="history">Liste der History-Einträge mit Zeit, gewachsenen und verbrannten Bäumen.</param>
	/// <param name="title">Titel der Linie, der in der Legende angezeigt wird.</param>
	/// <param name="color">Farbe der Durchschnittslinie.</param>
	/// <returns>Eine <see cref="LineSeries"/> für die Durchschnittslinie.</returns>
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
