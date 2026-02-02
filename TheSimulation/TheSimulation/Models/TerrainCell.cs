namespace TheSimulation;

/// <summary>
/// Repräsentiert die physikalischen und geografischen Eigenschaften einer einzelnen Gitterzelle im Gelände.
/// Da es als <see langword="struct"/> implementiert ist, ist es speichereffizient für große Karten.
/// </summary>
public struct TerrainCell
{
    /// <summary>
    /// Die relative Höhe der Zelle in einem Bereich von 0.0 bis 1.0.
    /// </summary>
    /// <remarks>
    /// 0.0 entspricht dem tiefsten Punkt (z. B. Meeresspiegel), 
    /// während 1.0 den höchsten Punkt (z. B. Bergspitze) darstellt. 
    /// Dieser Wert beeinflusst oft die Farbe (über den <see cref="ColorHelper"/>) 
    /// und kann die Brandausbreitung (Hangaufwärts brennt es schneller) steuern.
    /// </remarks>
    public float Elevation;

    /// <summary>
    /// Die Bodenbeschaffenheit oder der Bewuchs der Zelle.
    /// </summary>
    /// <remarks>
    /// Bestimmt, ob auf dieser Zelle Bäume wachsen können (z. B. <c>Forest</c>), 
    /// ob es sich um Wasser handelt oder ob die Zelle bereits abgebrannt ist.
    /// </remarks>
    public TerrainType Type;
}
