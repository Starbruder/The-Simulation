using System.Windows.Media;

namespace TheSimulation;

public static class Colors
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
}
