using Microsoft.Win32;
using OxyPlot;
using System.Windows;

namespace TheSimulation;

/// <summary>
/// Das Auswertungsfenster der Simulation. 
/// Visualisiert die gesammelten historischen Daten in Form von Liniendiagrammen und statistischen Kennzahlen.
/// </summary>
/// <remarks>
/// Nutzt die OxyPlot-Bibliothek zur Darstellung der Branddynamik und bietet eine Export-Schnittstelle für CSV-Daten.
/// </remarks>
public sealed partial class EvaluationWindow : Window
{
    /// <summary>
    /// Der zugrunde liegende Datensatz der abgeschlossenen oder pausierten Simulation.
    /// </summary>
    private readonly Evaluation data;

    /// <summary>
    /// Initialisiert eine neue Instanz des Auswertungsfensters und bereitet die Statistiken auf.
    /// </summary>
    /// <param name="data">Das <see cref="Evaluation"/>-Objekt mit allen historischen Snapshots der Simulation.</param>
    public EvaluationWindow(Evaluation data)
    {
        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);
        this.data = data;

        // Statische Wetterdaten und Laufzeit anzeigen
        EvalHumidity.Text = $"Air Humidity: {data.AirHumidityPercentage * 100:F0} %";
        EvalTemperature.Text = $"Air Temperature: {data.AirTemperatureCelsius}°C";

        // Durchschnittliche Windgeschwindigkeit berechnen und in Beaufort umrechnen
        var averageWindSpeed = data.History.Count > 0
            ? data.History.Average(h => h.WindSpeed)
            : 0;

        var windStrength = WindMapper.ConvertWindPercentStrenghToBeaufort(averageWindSpeed);

        EvalAverageWindSpeed.Text = data.History.Count > 0
            ? $"Average Wind Speed: {averageWindSpeed:P0} ({(int)windStrength} Bft)"
            : "Average Wind Speed: N/A";

        EvalRuntime.Text = $"Runtime: {data.Runtime:hh\\:mm\\:ss}";

        DrawCharts(data);
    }

    /// <summary>
    /// Prüft die Datenintegrität und stößt den Zeichenprozess für alle Diagramme an.
    /// </summary>
    /// <param name="data">Die zu visualisierenden Simulationsdaten.</param>
    private void DrawCharts(Evaluation data)
    {
        if (data.History.Count < 1)
        {
            MessageBox.Show("Havent yet collected enough infos to display charts.",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Zeichnen erst starten, wenn die UI-Elemente (PlotViews) bereit sind
        Loaded += (_, _) =>
        {
            DrawSimulationChart();
            DrawActiveTreesChart();
        };
    }

    /// <summary>
    /// Erstellt das Diagramm für die kumulativen Werte (Gerechnet über die gesamte Laufzeit).
    /// Zeigt den Vergleich zwischen insgesamt gewachsenen und insgesamt verbrannten Bäumen.
    /// </summary>
    private void DrawSimulationChart()
    {
        var model = ChartVisualizer.CreateLineChart("Grown / Burned Trees", "Time (s)", "Trees");

        var grown = ChartVisualizer.CreateLineSeries("Total Grown Trees", OxyColors.Green);
        var burned = ChartVisualizer.CreateLineSeries("Total Burned Trees", OxyColors.Red);

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

    /// <summary>
    /// Erstellt das Diagramm für den aktuellen Bestand ("Active Trees").
    /// Berechnet die Differenz zwischen gewachsenen und verbrannten Bäumen pro Zeitpunkt.
    /// </summary>
    private void DrawActiveTreesChart()
    {
        var model = ChartVisualizer.CreateLineChart("Active Trees", "Time (s)", "Trees");
        var active = ChartVisualizer.CreateLineSeries("Active Trees", OxyColors.DarkGreen);

        foreach (var h in data.History)
        {
            // Logik: Bestand = Alles was je gewachsen ist - Alles was bereits zerstört wurde
            var activeCount = (int)h.Grown - (int)h.Burned;
            active.Points.Add(new(h.Time.TotalSeconds, activeCount));
        }

        model.Series.Add(active);

        // Durchschnittslinie hinzufügen, um Trends besser erkennbar zu machen
        model.Series.Add(
            ChartVisualizer.CreateAverageLine(
                data.History, "Average Active Trees", OxyColors.Orange)
        );

        ActiveTreesPlot.Model = model;
    }

    /// <summary>
    /// Startet den Datei-Exportdialog und speichert die Simulationshistorie als CSV-Datei.
    /// </summary>
    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        if (data.History.Count == 0)
        {
            MessageBox.Show("No simulation data available to export.", "Export failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Export simulation evaluation",
            Filter = "CSV files (*.csv)|*.csv",
            FileName = $"ForestFireEvaluation_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            EvaluationExporter.ExportCsv(data, dialog.FileName);
            MessageBox.Show("CSV export completed successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Export error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
