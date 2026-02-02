using System.Windows.Media;

namespace TheSimulation;

/// <summary>
/// Stellt eine zentrale Sammlung von vordefinierten, eingefrorenen (frozen) Brushes 
/// für die visuelle Darstellung der Simulation bereit.
/// </summary>
public static class ColorsData
{
    /// <summary>
    /// Die Farbe für einen abgebrannten Baum (Asche).
    /// </summary>
    public static readonly Brush BurnedTreeColor = ColorHelper.GetFrozenBrush(Colors.Gray);

    /// <summary>
    /// Die Standardfarbe für aktives Feuer.
    /// </summary>
    public static readonly Brush DefaultFireColor = ColorHelper.GetFrozenBrush(Colors.OrangeRed);

    /// <summary>
    /// Die Farbe für Blitzeinschläge.
    /// </summary>
    public static readonly Brush LightningColor = ColorHelper.GetFrozenBrush(Colors.LightBlue);

    /// <summary>
    /// Die Farbe für den Vollbild-Blitzeffekt (Flash).
    /// </summary>
    public static readonly Brush FlashColor = ColorHelper.GetFrozenBrush(Colors.White);

    /// <summary>
    /// Eine Sammlung verschiedener Grüntöne zur Darstellungszwecken.
    /// </summary>
    public static readonly Brush[] TreeColors =
    [
        ColorHelper.GetFrozenBrush(Colors.Green),
        ColorHelper.GetFrozenBrush(Colors.DarkGreen),
        ColorHelper.GetFrozenBrush(Colors.YellowGreen),
        ColorHelper.GetFrozenBrush(Colors.ForestGreen)
    ];

    /// <summary>
    /// Eine Auswahl an feurigen Farbtönen zur Darstellung variabler Flammenintensitäten.
    /// </summary>
    public static readonly Brush[] FireColors =
    [
        DefaultFireColor,
        ColorHelper.GetFrozenBrush(Colors.DarkOrange),
        ColorHelper.GetFrozenBrush(Colors.Red),
        ColorHelper.GetFrozenBrush(Colors.Firebrick)
    ];
}
