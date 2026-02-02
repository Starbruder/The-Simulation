using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TheSimulation;

/// <summary>
/// Hält und verwaltet die Echtzeit-Statistiken der laufenden Simulation für die Datenbindung an die Benutzeroberfläche.
/// Implementiert <see cref="INotifyPropertyChanged"/>, um die UI automatisch bei Wertänderungen zu aktualisieren.
/// </summary>
public sealed class SimulationLiveStats : INotifyPropertyChanged
{
    private int _activeTrees;
    private int _maxTrees;
    private uint _totalGrown;
    private uint _totalBurned;
    private double _windStrength;
    private string _simulationTime = "00:00:00";

    /// <summary>Die Anzahl der aktuell im Wald befindlichen (lebenden) Bäume.</summary>
    public int ActiveTrees { get => _activeTrees; set => SetField(ref _activeTrees, value, [nameof(TreeDensityDisplay)]); }

    /// <summary>Die maximale Kapazität an Bäumen, die das Gelände fassen kann.</summary>
    public int MaxTrees { get => _maxTrees; set => SetField(ref _maxTrees, value, [nameof(TreeDensityDisplay)]); }

    /// <summary>Die kumulierte Anzahl aller jemals gewachsenen Bäume.</summary>
    public uint TotalGrown { get => _totalGrown; set => SetField(ref _totalGrown, value, [nameof(TotalGrownDisplay)]); }

    /// <summary>Die kumulierte Anzahl aller jemals verbrannten Bäume.</summary>
    public uint TotalBurned { get => _totalBurned; set => SetField(ref _totalBurned, value, [nameof(TotalBurnedDisplay)]); }

    /// <summary>Die aktuelle Windstärke als Normalwert (0.0 bis 1.0).</summary>
    public double WindStrength { get => _windStrength; set => SetField(ref _windStrength, value, [nameof(WindDisplay)]); }

    /// <summary>Die formatierte Laufzeit der aktuellen Simulationsinstanz.</summary>
    public string SimulationTime
    {
        get => _simulationTime;
        set
        {
            _simulationTime = value;
            OnPropertyChanged(nameof(SimulationTime));
        }
    }

    /// <summary>
    /// Gibt eine formatierte Zeichenfolge der aktuellen Baumdichte zurück (z. B. "450 / 1000 (45%)").
    /// </summary>
    public string TreeDensityDisplay =>
        $"{_activeTrees} / {_maxTrees} ({CalculatePercent(_activeTrees, _maxTrees):F0}%)";

    /// <summary>Gibt die Gesamtzahl der gewachsenen Bäume als Text zurück.</summary>
    public string TotalGrownDisplay => _totalGrown.ToString();

    /// <summary>Gibt die Gesamtzahl der verbrannten Bäume als Text zurück.</summary>
    public string TotalBurnedDisplay => _totalBurned.ToString();

    /// <summary>
    /// Bereitet die Windstärke für die Anzeige auf, inklusive Umrechnung in Prozent und Beaufort.
    /// </summary>
    public string WindDisplay
    {
        get
        {
            var percent = _windStrength * 100;
            var beaufort = (int)WindMapper.ConvertWindPercentStrenghToBeaufort(_windStrength);
            return $"{percent:F0}% ({beaufort} Bft)";
        }
    }

    /// <summary>
    /// Berechnet den prozentualen Anteil eines Wertes am Maximum.
    /// </summary>
    private static double CalculatePercent(int current, int max)
    {
        return max > 0
            ? (double)current / max * 100
            : 0;
    }

    /// <summary>Tritt ein, wenn sich ein Eigenschaftswert ändert.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Löst das <see cref="PropertyChanged"/>-Ereignis aus.</summary>
    /// <param name="propertyName">Name der geänderten Eigenschaft.</param>
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Setzt ein Feld und benachrichtigt die UI über Änderungen. 
    /// Ermöglicht zudem die Benachrichtigung von abhängigen "Display"-Eigenschaften.
    /// </summary>
    /// <typeparam name="T">Der Typ des Feldes.</typeparam>
    /// <param name="field">Referenz auf das zu ändernde Feld.</param>
    /// <param name="value">Der neue Wert.</param>
    /// <param name="affectedProperties">Optional: Namen weiterer Eigenschaften, die ebenfalls aktualisiert werden sollen.</param>
    /// <param name="name">Der Name der Haupteigenschaft (automatisch durch CallerMemberName).</param>
    /// <returns>True, wenn der Wert geändert wurde, sonst false.</returns>
    protected bool SetField<T>(ref T field, T value, string[] affectedProperties = null, [CallerMemberName] string name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name!);

        // Automatische Aktualisierung abhängiger UI-Strings (Magic)
        if (affectedProperties != null)
        {
            foreach (var prop in affectedProperties)
                OnPropertyChanged(prop);
        }

        return true;
    }
}
