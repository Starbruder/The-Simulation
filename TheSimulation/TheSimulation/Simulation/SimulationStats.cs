using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TheSimulation;

public sealed class SimulationStats : INotifyPropertyChanged
{
    private int _activeTrees;
    private int _maxTrees;
    private uint _totalGrown;
    private uint _totalBurned;
    private double _windStrength;
    private string _simulationTime = "00:00:00";

    public int ActiveTrees { get => _activeTrees; set => SetField(ref _activeTrees, value, [nameof(TreeDensityDisplay)]); }
    public int MaxTrees { get => _maxTrees; set => SetField(ref _maxTrees, value, [nameof(TreeDensityDisplay)]); }
    public uint TotalGrown { get => _totalGrown; set => SetField(ref _totalGrown, value, [nameof(TotalGrownDisplay)]); }
    public uint TotalBurned { get => _totalBurned; set => SetField(ref _totalBurned, value, [nameof(TotalBurnedDisplay)]); }
    public double WindStrength { get => _windStrength; set => SetField(ref _windStrength, value, [nameof(WindDisplay)]); }
    public string SimulationTime
    {
        get => _simulationTime;
        set
        {
            _simulationTime = value;
            OnPropertyChanged(nameof(SimulationTime));
        }
    }

    public string TreeDensityDisplay =>
        $"{_activeTrees} / {_maxTrees} ({CalculatePercent(_activeTrees, _maxTrees):F0}%)";

    public string TotalGrownDisplay => _totalGrown.ToString();

    public string TotalBurnedDisplay => _totalBurned.ToString();

    public string WindDisplay
    {
        get
        {
            var percent = _windStrength * 100;
            var beaufort = (int)WindMapper.ConvertWindPercentStrenghToBeaufort(_windStrength);
            return $"{percent:F0}% ({beaufort} Bft)";
        }
    }

    private static double CalculatePercent(int current, int max)
    {
        return max > 0
            ? (double)current / max * 100
            : 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, string[]? affectedProperties = null, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name!);

        // This is the magic: updating one value triggers updates for the "Display" strings
        if (affectedProperties != null)
        {
            foreach (var prop in affectedProperties)
                OnPropertyChanged(prop);
        }

        return true;
    }
}
