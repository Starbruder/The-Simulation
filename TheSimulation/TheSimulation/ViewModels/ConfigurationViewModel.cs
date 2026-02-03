using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TheSimulation;

/// <summary>
/// Represents the configuration and state for simulation settings, providing properties to control terrain generation,
/// forest growth, fire behavior, environmental conditions, and wind parameters. Supports property change notification
/// for data binding scenarios.
/// </summary>
/// <remarks>
/// This view model is typically used to bind simulation configuration options to a user interface. It
/// implements <see cref="INotifyPropertyChanged"/> to notify the UI of property changes. The <see
/// cref="ResetToDefaults"/> method restores all configuration properties to their default values. Some properties are
/// interdependent; for example, disabling forest growth automatically enables pausing tree growth during fire.
/// Properties such as <see cref="IsPauseFireEnabled"/> and <see cref="IsWindDirectionBoxEnabled"/> are provided to
/// facilitate UI logic based on other settings.
/// </remarks>
public sealed class ConfigurationViewModel : INotifyPropertyChanged
{
    // Terrain
    private bool _useTerrainGeneration = true;

    /// <summary>
    /// Gets or sets a value indicating whether terrain generation is enabled for the simulation.
    /// </summary>
    public bool UseTerrainGeneration
    {
        get => _useTerrainGeneration;
        set => SetField(ref _useTerrainGeneration, value);
    }

    // Trees
    private bool _growForest = true;

    /// <summary>
    /// Gets or sets a value indicating whether forest growth is active. 
    /// Disabling this will automatically set <see cref="PauseGrowingDuringFire"/> to true.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the initial density of the forest when the simulation starts.
    /// </summary>
    public double PrefillDensity
    {
        get => _prefillDensity;
        set => SetField(ref _prefillDensity, value);
    }

    // Fire
    private bool _pauseGrowingDuringFire = true;

    /// <summary>
    /// Gets or sets a value indicating whether tree growth should be suspended while a fire is active.
    /// </summary>
    public bool PauseGrowingDuringFire
    {
        get => _pauseGrowingDuringFire;
        set => SetField(ref _pauseGrowingDuringFire, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the "Pause Fire" setting is enabled in the UI. 
    /// This is internally coupled with <see cref="GrowForest"/>.
    /// </summary>
    public bool IsPauseFireEnabled
    {
        get => GrowForest;
        set => GrowForest = value;
    }

    private double _fireSpreadChance = 40;

    /// <summary>
    /// Gets or sets the probability (as a percentage) of fire spreading to adjacent cells.
    /// </summary>
    public double FireSpreadChance
    {
        get => _fireSpreadChance;
        set => SetField(ref _fireSpreadChance, value);
    }

    private double _lightningChance = 15;

    /// <summary>
    /// Gets or sets the probability of a lightning strike occurring, which can trigger fires.
    /// </summary>
    public double LightningChance
    {
        get => _lightningChance;
        set => SetField(ref _lightningChance, value);
    }

    // Environment
    private double _airHumidity = 50;

    /// <summary>
    /// Gets or sets the simulated air humidity level.
    /// </summary>
    public double AirHumidity
    {
        get => _airHumidity;
        set => SetField(ref _airHumidity, value);
    }

    private double _airTemperature = 30;

    /// <summary>
    /// Gets or sets the simulated air temperature.
    /// </summary>
    public double AirTemperature
    {
        get => _airTemperature;
        set => SetField(ref _airTemperature, value);
    }

    private bool _randomWindDirection = false;

    /// <summary>
    /// Gets or sets a value indicating whether the wind direction is randomized during the simulation.
    /// </summary>
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

    /// <summary>
    /// Provides a collection of all available wind directions defined in the <see cref="WindDirection"/> enum.
    /// </summary>
    public static IEnumerable<WindDirection> WindDirections => Enum.GetValues<WindDirection>();

    private bool _isWindDirectionBoxEnabled = true;

    /// <summary>
    /// Gets or sets a value indicating whether the wind direction selection control should be enabled in the UI.
    /// </summary>
    public bool IsWindDirectionBoxEnabled
    {
        get => _isWindDirectionBoxEnabled;
        set => SetField(ref _isWindDirectionBoxEnabled, value);
    }

    private WindDirection _selectedWindDirection = SimulationDefaultsData.DefaultWindDirection;

    /// <summary>
    /// Gets or sets the currently selected wind direction.
    /// </summary>
    public WindDirection SelectedWindDirection
    {
        get => _selectedWindDirection;
        set => SetField(ref _selectedWindDirection, value);
    }

    private bool _randomWindStrength = false;

    /// <summary>
    /// Gets or sets a value indicating whether the wind strength should vary randomly.
    /// </summary>
    public bool RandomWindStrength
    {
        get => _randomWindStrength;
        set => SetField(ref _randomWindStrength, value);
    }

    private double _windStrength = 0.75;

    /// <summary>
    /// Gets or sets the wind strength value (typically between 0 and 1).
    /// </summary>
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

    /// <summary>
    /// Resets all configuration properties to their predefined default values.
    /// </summary>
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

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Sets a field and raises the <see cref="PropertyChanged"/> event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">A reference to the backing field.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>True if the value was changed; otherwise false.</returns>
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
