using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TheSimulation;

/// <summary>
/// Das ViewModel für die Grafikeinstellungen. 
/// Kapselt die Logik zur Bearbeitung und zum Zurücksetzen von Grafikeigenschaften.
/// </summary>
public sealed class GraphicsViewModel : INotifyPropertyChanged
{
    private GraphicsSettings workingSettings;

    /// <summary>
    /// Ruft die aktuellen Einstellungen ab, mit denen die Benutzeroberfläche arbeitet, oder legt diese fest.
    /// </summary>
    public GraphicsSettings WorkingSettings
    {
        get => workingSettings;
        set
        {
            workingSettings = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Ruft alle verfügbaren Baumformen für die Auswahl (z. B. in einer ComboBox) ab.
    /// </summary>
    public static IEnumerable<TreeShapeType> AvailableTreeShapes => Enum.GetValues<TreeShapeType>();

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="GraphicsViewModel"/> Klasse.
    /// Erstellt eine Arbeitskopie der übergebenen Einstellungen.
    /// </summary>
    /// <param name="currentSettings">Die aktuell aktiven Einstellungen, die als Basis dienen.</param>
    public GraphicsViewModel(GraphicsSettings currentSettings)
    {
        // Wir arbeiten auf einer Kopie, damit "Abbrechen" (Fenster schließen) 
        // die Originaleinstellungen nicht verändert.
        WorkingSettings = currentSettings.Duplicate();
    }

    /// <summary>
    /// Setzt die <see cref="WorkingSettings"/> auf ihre Standardwerte zurück.
    /// </summary>
    public void Reset()
    {
        WorkingSettings.ResetToDefaults();
        // Damit das UI merkt, dass sich die Werte IM Objekt geändert haben:
        OnPropertyChanged(nameof(WorkingSettings));
    }

    /// <summary>
    /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Benachrichtigt Abonnenten über eine Änderung einer Eigenschaft.
    /// </summary>
    /// <param name="propertyName">Name der Eigenschaft. Wird automatisch durch <see cref="CallerMemberNameAttribute"/> gefüllt.</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged.Invoke(this, new(propertyName));
}
