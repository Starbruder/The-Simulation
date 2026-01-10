using System.Windows.Media;

namespace TheSimulation;

public static class ColorHelper
{
    public static Brush[] TreeColors
    {
        get => [
            Brushes.Green,       // Kiefer
            Brushes.DarkGreen,   // Eiche
            Brushes.YellowGreen, // Buche
            Brushes.ForestGreen  // Tanne
        ];
    }

    public static Brush AdjustColorByElevation(Brush baseColor, float elevation)
    {
        // elevation: 0.0 (tief) bis 1.0 (hoch)
        var sc = ((SolidColorBrush)baseColor).Color;

        // Helligkeit linear anpassen: tief = dunkel, hoch = hell
        byte Adjust(byte channel) =>
            (byte)Math.Clamp(channel * 0.5 + 0.5 * channel * elevation, 0, 255);

        var newColor = Color.FromRgb(
            Adjust(sc.R),
            Adjust(sc.G),
            Adjust(sc.B)
        );

        return new SolidColorBrush(newColor);
    }
}
