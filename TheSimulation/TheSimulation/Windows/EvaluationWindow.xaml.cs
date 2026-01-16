using OxyPlot;
using System.Windows;

namespace TheSimulation;

/// <summary>
/// Interaktionslogik für EvaluationWindow.xaml
/// </summary>
public sealed partial class EvaluationWindow : Window
{
    private readonly Evaluation data;

    public EvaluationWindow(Evaluation data)
    {
        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);
        this.data = data;

        EvalHumidity.Text = $"Air Humidity: {data.AirHumidityPercentage * 100:F0} %";
        EvalTemperature.Text = $"Air Temperature: {data.AirTemperatureCelsius}°C";

        var averageWindSpeed = data.History.Count > 0
            ? data.History.Average(h => h.WindSpeed)
            : 0.0;

        var windStrength = WindMapper.ConvertWindPercentStrenghToBeaufort(averageWindSpeed);

        EvalAverageWindSpeed.Text = data.History.Count > 0
            ? $"Average Wind Speed: {averageWindSpeed:P0} ({(int)windStrength} Bft)"
            : "Average Wind Speed: N/A";

        EvalRuntime.Text = $"Runtime: {data.Runtime:hh\\:mm\\:ss}";

        DrawCharts(data);
    }

    private void DrawCharts(Evaluation data)
    {
        if (data.History.Count < 1)
        {
            MessageBox.Show("Havent yet collected enough infos to display charts.",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

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
