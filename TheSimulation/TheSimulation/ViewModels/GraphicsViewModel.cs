using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TheSimulation;

public class GraphicsViewModel : INotifyPropertyChanged
{
    private GraphicsSettings workingSettings;

    // Das sind die Einstellungen, mit denen das UI arbeitet
    public GraphicsSettings WorkingSettings
    {
        get => workingSettings;
        set
        {
            workingSettings = value;
            OnPropertyChanged();
        }
    }

    // Liste für die ComboBox
    public static IEnumerable<TreeShapeType> AvailableTreeShapes => Enum.GetValues<TreeShapeType>();

    public GraphicsViewModel(GraphicsSettings currentSettings)
    {
        // Wir arbeiten auf einer Kopie, damit "Abbrechen" (Fenster schließen) 
        // die Originaleinstellungen nicht verändert.
        WorkingSettings = currentSettings.Duplicate();
    }

    public void Reset()
    {
        WorkingSettings.ResetToDefaults();
        // Damit das UI merkt, dass sich die Werte IM Objekt geändert haben:
        OnPropertyChanged(nameof(WorkingSettings));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }
}
