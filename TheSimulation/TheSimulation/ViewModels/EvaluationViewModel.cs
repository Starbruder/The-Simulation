using OxyPlot;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TheSimulation;

/// <summary>
/// Das ViewModel für die Auswertung der Simulationsergebnisse.
/// Bereitet Daten für die grafische Darstellung mit OxyPlot und die Anzeige in der UI auf.
/// </summary>
public class EvaluationViewModel : INotifyPropertyChanged
{
    private PlotModel grownBurnedModel;
    private PlotModel activeTreesModel;
    private string humidityText;
    private string temperatureText;
    private string windSpeedText;
    private string runtimeText;

    /// <summary>
    /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Holt oder setzt das PlotModel für den Vergleich von gewachsenen und verbrannten Bäumen.
    /// </summary>
    public PlotModel GrownBurnedModel
    {
        get => grownBurnedModel;
        set => SetField(ref grownBurnedModel, value);
    }

    /// <summary>
    /// Holt oder setzt das PlotModel für die Anzahl der aktuell aktiven Bäume über die Zeit.
    /// </summary>
    public PlotModel ActiveTreesModel
    {
        get => activeTreesModel;
        set => SetField(ref activeTreesModel, value);
    }

    /// <summary>
    /// Formatiert den Text für die Luftfeuchtigkeit zur Anzeige in der UI.
    /// </summary>
    public string HumidityText
    {
        get => humidityText;
        set => SetField(ref humidityText, value);
    }

    /// <summary>
    /// Formatiert den Text für die Lufttemperatur zur Anzeige in der UI.
    /// </summary>
    public string TemperatureText
    {
        get => temperatureText;
        set => SetField(ref temperatureText, value);
    }

    /// <summary>
    /// Formatiert den Text für die durchschnittliche Windgeschwindigkeit (inkl. Beaufort-Skala).
    /// </summary>
    public string WindSpeedText
    {
        get => windSpeedText;
        set => SetField(ref windSpeedText, value);
    }

    /// <summary>
    /// Formatiert die Laufzeit der Simulation als Text.
    /// </summary>
    public string RuntimeText
    {
        get => runtimeText;
        set => SetField(ref runtimeText, value);
    }

    private readonly Evaluation _data;

    /// <summary>
    /// Zugriff auf das zugrunde liegende Datenobjekt für Exportzwecke.
    /// </summary>
    public Evaluation Data => _data;

    /// <summary>
    /// Gibt die Anzahl der Einträge in der Historie zurück.
    /// </summary>
    public int HistoryCount => _data.History?.Count ?? 0;

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="EvaluationViewModel"/> Klasse.
    /// </summary>
    /// <param name="data">Das <see cref="Evaluation"/> Datenobjekt aus der Simulation.</param>
    public EvaluationViewModel(Evaluation data)
    {
        _data = data;
        InitializeData(data);
    }

    /// <summary>
    /// Bereitet die Rohdaten für die Bindung an die View vor, inkl. Textformatierung und Diagrammerstellung.
    /// </summary>
    /// <param name="data">Die zu visualisierenden Simulationsdaten.</param>
    private void InitializeData(Evaluation data)
    {
        // Texte aufbereiten
        HumidityText = $"Air Humidity: {data.AirHumidityPercentage * 100:F0} %";
        TemperatureText = $"Air Temperature: {data.AirTemperatureCelsius}°C";
        RuntimeText = $"Runtime: {data.Runtime:hh\\:mm\\:ss}";

        var averageWindSpeed = data.History.Count > 0 ? data.History.Average(h => h.WindSpeed) : 0;
        var windStrength = WindMapper.ConvertWindPercentStrenghToBeaufort(averageWindSpeed);
        WindSpeedText = data.History.Count > 0
            ? $"Average Wind Speed: {averageWindSpeed:P0} ({(int)windStrength} Bft)"
            : "Average Wind Speed: N/A";

        // Charts generieren
        if (data.History.Count > 0)
        {
            GrownBurnedModel = CreateGrownBurnedModel(data);
            ActiveTreesModel = CreateActiveTreesModel(data);
        }
    }

    /// <summary>
    /// Erstellt das PlotModel für das "Grown vs. Burned" Liniendiagramm.
    /// </summary>
    /// <param name="data">Die Simulationsdaten.</param>
    /// <returns>Ein konfiguriertes <see cref="PlotModel"/>.</returns>
    private static PlotModel CreateGrownBurnedModel(Evaluation data)
    {
        var model = ChartVisualizer.CreateLineChart("Grown / Burned Trees", "Time (s)", "Trees");
        var grown = ChartVisualizer.CreateLineSeries("Total Grown Trees", OxyColors.Green);
        var burned = ChartVisualizer.CreateLineSeries("Total Burned Trees", OxyColors.Red);

        foreach (var snapshot in data.History)
        {
            var t = snapshot.Time.TotalSeconds;
            grown.Points.Add(new(t, snapshot.Grown));
            burned.Points.Add(new(t, snapshot.Burned));
        }

        model.Series.Add(grown);
        model.Series.Add(burned);
        return model;
    }

    /// <summary>
    /// Erstellt das PlotModel für die aktiven Bäume inkl. einer Durchschnittslinie.
    /// </summary>
    /// <param name="data">Die Simulationsdaten.</param>
    /// <returns>Ein konfiguriertes <see cref="PlotModel"/>.</returns>
    private static PlotModel CreateActiveTreesModel(Evaluation data)
    {
        var model = ChartVisualizer.CreateLineChart("Active Trees", "Time (s)", "Trees");
        var active = ChartVisualizer.CreateLineSeries("Active Trees", OxyColors.DarkGreen);

        foreach (var h in data.History)
        {
            var activeCount = (int)h.Grown - (int)h.Burned;
            active.Points.Add(new(h.Time.TotalSeconds, activeCount));
        }

        model.Series.Add(active);
        model.Series.Add(ChartVisualizer.CreateAverageLine(data.History, "Average Active Trees", OxyColors.Orange));
        return model;
    }

    /// <summary>
    /// Löst das PropertyChanged-Ereignis aus.
    /// </summary>
    /// <param name="propertyName">Name der geänderten Eigenschaft (automatisch durch CallerMemberName).</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new(propertyName));

    /// <summary>
    /// Vergleicht den aktuellen Wert eines Feldes mit einem neuen Wert und aktualisiert diesen bei Bedarf.
    /// Löst danach <see cref="OnPropertyChanged"/> aus.
    /// </summary>
    /// <typeparam name="T">Der Typ des Feldes.</typeparam>
    /// <param name="field">Referenz auf das private Feld.</param>
    /// <param name="value">Der neue Wert.</param>
    /// <param name="propertyName">Name der Eigenschaft.</param>
    /// <returns>True, wenn der Wert geändert wurde, sonst false.</returns>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
