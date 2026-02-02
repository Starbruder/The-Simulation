using System.Windows;
using System.Windows.Shapes;

namespace TheSimulation;

/// <summary>
/// Repräsentiert ein flüchtiges Partikel-Objekt in der Simulationsgrafik (z. B. Funken, Rauch oder Asche).
/// Kombiniert visuelle Darstellung mit physikalischen Eigenschaften wie Geschwindigkeit und Lebensdauer.
/// </summary>
/// <remarks>
/// Dieses <see langword="record"/> wird für kurzlebige visuelle Effekte genutzt. 
/// Sobald die <see cref="Lifetime"/> den Wert 0 erreicht, wird das Partikel normalerweise vom Renderer entfernt.
/// </remarks>
/// <param name="Visual">Das WPF-Shape (meist eine kleine Ellipse), das auf dem Canvas gerendert wird.</param>
/// <param name="Velocity">Ein Richtungsvektor, der die Bewegung des Partikels pro Frame angibt.</param>
/// <param name="Lifetime">Die verbleibende Zeit (oder Anzahl an Frames), die das Partikel existiert.</param>
public sealed record Particle(Ellipse Visual, Vector Velocity, double Lifetime)
{
    /// <summary>
    /// Ruft die aktuelle Lebensdauer des Partikels ab.
    /// </summary>
    /// <value>
    /// Ein Wert größer als 0 bedeutet, dass das Partikel aktiv ist. Bei 0 oder weniger sollte es gelöscht werden.
    /// </value>
    public double Lifetime { get; private set; } = Lifetime;

    /// <summary>
    /// Verringert die Lebensdauer des Partikels um einen bestimmten Betrag.
    /// Wird typischerweise in jedem Update-Zyklus der Animation aufgerufen.
    /// </summary>
    /// <param name="amount">Der Wert, um den die Lebensdauer reduziert werden soll (z. B. verstrichene Zeit oder ein fester Wert pro Frame).</param>
    public void DecreaseLifetime(double amount)
    {
        Lifetime -= amount;
    }
}
