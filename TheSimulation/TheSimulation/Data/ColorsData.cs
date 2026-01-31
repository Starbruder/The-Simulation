using System.Windows.Media;

namespace TheSimulation;

public static class ColorsData
{
    public static readonly Brush BurnedTreeColor = ColorHelper.GetFrozenBrush(Colors.Gray);
    public static readonly Brush DefaultFireColor = ColorHelper.GetFrozenBrush(Colors.OrangeRed);
    public static readonly Brush LightningColor = ColorHelper.GetFrozenBrush(Colors.LightBlue);
    public static readonly Brush FlashColor = ColorHelper.GetFrozenBrush(Colors.White);

    public static readonly Brush[] TreeColors =
    [
        ColorHelper.GetFrozenBrush(Colors.Green),       // Kiefer
        ColorHelper.GetFrozenBrush(Colors.DarkGreen),   // Eiche
        ColorHelper.GetFrozenBrush(Colors.YellowGreen), // Buche
        ColorHelper.GetFrozenBrush(Colors.ForestGreen)  // Tanne
    ];

    public static readonly Brush[] FireColors =
    [
        DefaultFireColor,
        ColorHelper.GetFrozenBrush(Colors.DarkOrange),
        ColorHelper.GetFrozenBrush(Colors.Red),
        ColorHelper.GetFrozenBrush(Colors.Firebrick)
    ];
}
