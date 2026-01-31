using System.Windows.Media;

namespace TheSimulation;

public static class ColorsData
{
    //public static Brush[] TreeGridColorBrushes
    //{
    //    get => [
    //        Brushes.Green,       // Kiefer
    //        Brushes.DarkGreen,   // Eiche
    //        Brushes.YellowGreen, // Buche
    //        Brushes.ForestGreen  // Tanne
    //    ];
    //}

    public static readonly uint[] TreeGridColors =
    [
        0xFF008000, // Green (Kiefer)
        0xFF006400, // DarkGreen (Eiche)
        0xFF9ACD32, // YellowGreen (Buche)
        0xFF228B22  // ForestGreen (Tanne)
    ];

    //public static Brush BurnedGridTreeColorBrush = Brushes.Gray;
    public const uint BurnedGridTreeColor = 0xFF555555; // Dunkelgrau (Asche)


    //public static Brush LightningGridColorBrush = Brushes.LightBlue;
    public const uint LightningGridColor = 0xFFADD8E6; // LightBlue

    public static Brush[] FireColorsBrushes
    {
        get => [
            Brushes.OrangeRed,
            Brushes.DarkOrange,
            Brushes.Red,
            Brushes.Firebrick
        ];
    }

    public static readonly uint[] FireColors =
    [
        0xFFFF4500, // OrangeRed
        0xFFFF8C00, // DarkOrange
        0xFFFF0000, // Red
        0xFFB22222  // Firebrick
    ];

    public const uint SoilColor = 0xFF8B4513;  // Braun
    public const uint RockColor = 0xFF808080;  // Grau
    public const uint WaterColor = 0xFF0000FF; // Blau
}
