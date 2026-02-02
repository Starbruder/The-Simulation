using System.Windows.Media;

namespace TheSimulation;

public static class ColorHelper
{
    // Cache für Höhen-Farben: Key ist eine Kombination aus Basis-Farbe und Elevation
    private static readonly Dictionary<(Color, int), Brush> _elevationCache = [];

    /// <summary>
    /// Creates a new solid color brush with the specified color and returns a frozen (read-only) instance for improved
    /// performance.
    /// </summary>
    /// <remarks>Freezing the brush makes it immutable and can improve performance, especially when the brush
    /// is used in multiple places or across threads.</remarks>
    /// <param name="color">The color to use for the solid color brush.</param>
    /// <returns>A frozen <see cref="SolidColorBrush"/> with the specified color.
    /// The returned brush is read-only and can be safely shared
    /// across threads.</returns>
    public static Brush GetFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    /// <summary>
    /// Creates a new brush with its color brightness adjusted according to the specified elevation value.
    /// </summary>
    /// <remarks>This method linearly interpolates the brightness of the base color according to the elevation
    /// parameter. The returned brush is optimized for performance by being frozen.</remarks>
    /// <param name="baseColor">The base brush whose color will be adjusted. Must be a non-null instance of SolidColorBrush.</param>
    /// <param name="elevation">A value between 0.0 and 1.0 representing the elevation, where 0.0 produces a darker color and 1.0 produces a
    /// lighter color.</param>
    /// <returns>A new SolidColorBrush with its color brightness modified based on the elevation value. The returned brush is
    /// frozen and cannot be modified.</returns>
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
