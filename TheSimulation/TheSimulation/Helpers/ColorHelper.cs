using System.Windows.Media;

namespace TheSimulation;

/// <summary>
/// Hilfsklasse für die Farbverwaltung und grafische Optimierung.
/// Bietet Funktionen zum Einfrieren von Brushes und zur dynamischen Helligkeitsanpassung basierend auf Geländehöhen.
/// </summary>
public static class ColorHelper
{
    /// <summary>
    /// Stellt einen Cache bereit, der eine Kombination aus Basisfarbe und Höhenwert auf eine entsprechende Brush-Instanz abbildet.
    /// </summary>
    /// <remarks>
    /// Dieser Cache wird verwendet, um die Performance zu verbessern, indem Brush-Instanzen für wiederholte Farb- und 
    /// Höhenkombinationen wiederverwendet werden.
    /// </remarks>
    private static readonly Dictionary<(Color, int), Brush> _elevationCache = [];

    /// <summary>
    /// Erstellt einen neuen SolidColorBrush mit der angegebenen Farbe und gibt eine eingefrorene (schreibgeschützte) Instanz zurück.
    /// </summary>
    /// <remarks>
    /// Das Einfrieren (Freeze) macht den Brush unveränderlich und verbessert die Performance erheblich.
    /// </remarks>
    /// <param name="color">Die Farbe, die für den Brush verwendet werden soll.</param>
    /// <returns>Ein eingefrorener <see cref="SolidColorBrush"/>. Dieser ist schreibgeschützt.</returns>
    public static Brush GetFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    /// <summary>
    /// Erstellt einen neuen Brush, dessen Helligkeit basierend auf einem angegebenen Höhenwert (Elevation) angepasst wurde.
    /// </summary>
    /// <remarks>
    /// Diese Methode berechnet die Helligkeit der Basisfarbe linear in Abhängigkeit vom Höhenparameter. 
    /// Der zurückgegebene Brush ist für eine bessere Performance eingefroren.
    /// </remarks>
    /// <param name="baseBrush">Der Basis-Brush, dessen Farbe angepasst werden soll. Muss eine Instanz von SolidColorBrush sein.</param>
    /// <param name="elevation">Ein Wert zwischen 0.0 und 1.0, der die Höhe repräsentiert (0.0 = dunkler, 1.0 = heller/original).</param>
    /// <returns>Ein neuer, eingefrorener SolidColorBrush mit angepasster Helligkeit.</returns>
    public static Brush AdjustColorByElevation(Brush baseBrush, float elevation)
    {
        var baseColor = ((SolidColorBrush)baseBrush).Color;

        // Wir runden die Elevation auf 2 Dezimalstellen (0.01 Schritte), 
        // damit der Cache nicht für winzigste Unterschiede explodiert.
        var elevationKey = (int)(elevation * 100);
        var key = (baseColor, elevationKey);

        if (_elevationCache.TryGetValue(key, out var cachedBrush))
        {
            return cachedBrush;
        }

        byte Adjust(byte channel) =>
            (byte)Math.Clamp(channel * 0.5 + 0.5 * channel * elevation, 0, 255);

        var newColor = Color.FromRgb(Adjust(baseColor.R), Adjust(baseColor.G), Adjust(baseColor.B));
        var newBrush = GetFrozenBrush(newColor);

        _elevationCache[key] = newBrush;

        return newBrush;
    }
}
