using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TheSimulation;

/// <summary>
/// Represents the configuration and state for simulation settings, providing properties to control terrain generation,
/// forest growth, fire behavior, environmental conditions, and wind parameters. Supports property change notification
/// for data binding scenarios.
/// </summary>
/// <remarks>This view model is typically used to bind simulation configuration options to a user interface. It
/// implements <see cref="INotifyPropertyChanged"/> to notify the UI of property changes. The <see
/// cref="ResetToDefaults"/> method restores all configuration properties to their default values. Some properties are
/// interdependent; for example, disabling forest growth automatically enables pausing tree growth during fire.
/// Properties such as <see cref="IsPauseFireEnabled"/> and <see cref="IsWindDirectionBoxEnabled"/> are provided to
/// facilitate UI logic based on other settings.</remarks>
public sealed class ConfigurationViewModel : INotifyPropertyChanged
{
    // Terrain
    private bool _useTerrainGeneration = true;
    public bool UseTerrainGeneration
    {
        get => _useTerrainGeneration;
        set => SetField(ref _useTerrainGeneration, value);
    }

    // Trees
    private bool _growForest = true;
    public bool GrowForest
    {
        get => _growForest;
        set
        {
            if (SetField(ref _growForest, value))
            {
                // Logik-Kopplung: Wenn GrowForest aus, muss PauseFire an sein
                if (!value)
                {
                    PauseGrowingDuringFire = true;
                }

                OnPropertyChanged(nameof(IsPauseFireEnabled));
            }
        }
    }

    private double _prefillDensity = 80;
    public double PrefillDensity
    {
        get => _prefillDensity;
        set => SetField(ref _prefillDensity, value);
    }

    // Fire
    private bool _pauseGrowingDuringFire = true;
    public bool PauseGrowingDuringFire
    {
        get => _pauseGrowingDuringFire;
        set => SetField(ref _pauseGrowingDuringFire, value);
    }

    public bool IsPauseFireEnabled
    {
        get => GrowForest;
        set => GrowForest = value;
    }

    private double _fireSpreadChance = 40;
    public double FireSpreadChance
    {
        get => _fireSpreadChance;
        set => SetField(ref _fireSpreadChance, value);
    }

    private double _lightningChance = 15;
    public double LightningChance
    {
        get => _lightningChance;
        set => SetField(ref _lightningChance, value);
    }

    // Environment
    private double _airHumidity = 50;
    public double AirHumidity
    {
        get => _airHumidity;
        set => SetField(ref _airHumidity, value);
    }

    private double _airTemperature = 30;
    public double AirTemperature
    {
        get => _airTemperature;
        set => SetField(ref _airTemperature, value);
    }

    private bool _randomWindDirection = false;
    public bool RandomWindDirection
    {
        get => _randomWindDirection;
        set
        {
            if (SetField(ref _randomWindDirection, value))
            {
                OnPropertyChanged(nameof(IsWindDirectionBoxEnabled));
            }
        }
    }

    public static IEnumerable<WindDirection> WindDirections => Enum.GetValues<WindDirection>();

    private bool _isWindDirectionBoxEnabled = true;
    public bool IsWindDirectionBoxEnabled
    {
        get => _isWindDirectionBoxEnabled;
        set => SetField(ref _isWindDirectionBoxEnabled, value);
    }

    private WindDirection _selectedWindDirection = SimulationDefaultsData.DefaultWindDirection;
    public WindDirection SelectedWindDirection { get => _selectedWindDirection; set => SetField(ref _selectedWindDirection, value); }

    private bool _randomWindStrength = false;
    public bool RandomWindStrength
    {
        get => _randomWindStrength;
        set => SetField(ref _randomWindStrength, value);
    }

    private double _windStrength = 0.75;
    public double WindStrength
    {
        get => _windStrength;
        set
        {
            if (SetField(ref _windStrength, value))
            {
                OnPropertyChanged(nameof(WindStrengthDescription));
            }
        }
    }

    /// <summary>
    /// Liefert eine formatierte Zusammenfassung der Windstärke (z. B. Prozentwert und Beaufort-Skala).
    /// </summary>
    /// <remarks>
    /// Diese Eigenschaft ist schreibgeschützt und wird automatisch berechnet. 
    /// Um den zugrunde liegenden Wert zu ändern, muss die Eigenschaft <see cref="WindStrength"/> verwendet werden.
    /// </remarks>
    public string WindStrengthDescription =>
        $"Wind Strength: {WindStrength * 100:F0}% ({(int)WindMapper.ConvertWindPercentStrenghToBeaufort(WindStrength)} Bft)";

    public void ResetToDefaults()
    {
        UseTerrainGeneration = true;
        GrowForest = true;
        PrefillDensity = 80;
        PauseGrowingDuringFire = true;
        FireSpreadChance = 40;
        LightningChance = 15;
        AirHumidity = 50;
        AirTemperature = 30;
        RandomWindDirection = false;
        SelectedWindDirection = SimulationDefaultsData.DefaultWindDirection;
        RandomWindStrength = false;
        WindStrength = 0.75;
    }

    // PropertyChanged Infrastruktur
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
