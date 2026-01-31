using System.Windows.Media;

namespace TheSimulation;

public static class ColorsData
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

    public static Brush BurnedTreeColor = Brushes.Gray;

    public static Brush[] FireColors
    {
        get => [
            Brushes.OrangeRed,
            Brushes.DarkOrange,
            Brushes.Red,
            Brushes.Firebrick
        ];
    }

    public static Brush LightningColor = Brushes.LightBlue;
}
