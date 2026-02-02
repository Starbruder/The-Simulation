using System.Windows.Media;

namespace TheSimulation;

/// <summary>
/// Hilfsklasse zur Erzeugung von Zufallswerten für die Simulation.
/// Kapselt die Standard-Random-Logik und bietet spezialisierte Methoden für Zellen und Farben.
/// </summary>
public sealed class RandomHelper
{
    // Nutzt den Standard-Random-Generator aus System.Random.
    private readonly Random random = new();

    /// <summary>
    /// Erzeugt eine ganzzahlige Zufallszahl innerhalb eines angegebenen Bereichs.
    /// </summary>
    /// <param name="minValue">Die inklusive Untergrenze.</param>
    /// <param name="maxValue">Die exklusive Obergrenze.</param>
    /// <returns>Ein zufälliger Integer-Wert.</returns>
    public int NextInt(int minValue, int maxValue)
        => random.Next(minValue, maxValue);

    /// <summary>
    /// Erzeugt eine Gleitkommazahl zwischen 0.0 und 1.0.
    /// </summary>
    /// <returns>Ein zufälliger Double-Wert.</returns>
    public double NextDouble()
        => random.NextDouble();

    /// <summary>
    /// Erzeugt eine Gleitkommazahl innerhalb eines spezifischen Bereichs.
    /// </summary>
    /// <param name="minValue">Die Untergrenze des Bereichs.</param>
    /// <param name="maxValue">Die Obergrenze des Bereichs.</param>
    /// <returns>Ein zufälliger skalierter Double-Wert.</returns>
    public double NextDouble(float minValue, float maxValue)
        => random.NextDouble() * (maxValue - minValue) + minValue;

    /// <summary>
    /// Wählt eine zufällige Koordinate (Zelle) innerhalb der Gitter-Dimensionen aus.
    /// </summary>
    /// <param name="columns">Anzahl der Spalten im Gitter.</param>
    /// <param name="rows">Anzahl der Zeilen im Gitter.</param>
    /// <returns>Eine neue <see cref="Cell"/> mit zufälligen X- und Y-Koordinaten.</returns>
    public Cell NextCell(int columns, int rows)
        => new(NextInt(0, columns), NextInt(0, rows));

    /// <summary>
    /// Wählt zufällig eine Baumfarbe aus den vordefinierten globalen Farbdaten aus.
    /// </summary>
    /// <remarks>
    /// Um die Rendering-Performance zu maximieren, wird sichergestellt, dass der zurückgegebene 
    /// Brush eingefroren (frozen) ist. Dies ist besonders wichtig für WPF, um den UI-Thread zu entlasten.
    /// </remarks>
    /// <returns>Ein eingefrorener <see cref="Brush"/>, der eine zufällige Baumfarbe repräsentiert.</returns>
    public Brush NextTreeColor()
    {
        var index = NextInt(0, ColorsData.TreeColors.Length);
        var frozenTreeColor = ColorsData.TreeColors[index];

        // Brushes sollten idealerweise bereits in ColorsData gefroren sein,
        // aber wir stellen es hier für die Threadsicherheit sicher.
        if (frozenTreeColor.CanFreeze && !frozenTreeColor.IsFrozen)
        {
            frozenTreeColor.Freeze();
        }
        return frozenTreeColor;
    }

    /// <summary>
    /// Wählt eine zufällige Zelle aus einer Menge (HashSet) von verfügbaren Zellen aus.
    /// </summary>
    /// <param name="cell">Die Menge der zur Auswahl stehenden Zellen (z. B. wachstumsfähige Zellen).</param>
    /// <returns>Eine zufällig gewählte <see cref="Cell"/> aus der Menge.</returns>
    /// <exception cref="InvalidOperationException">Wird geworfen, wenn die Menge leer ist.</exception>
    public Cell NextCell(HashSet<Cell> cell)
    {
        if (cell.Count == 0)
        {
            throw new InvalidOperationException("Keine verfügbaren Zellen für diese Operation vorhanden.");
        }

        // Hinweis: ElementAt ist bei HashSets O(n). Bei sehr großen Mengen 
        // könnte man über eine alternative Listen-Struktur nachdenken.
        var randomIndex = NextInt(0, cell.Count);
        return cell.ElementAt(randomIndex);
    }
}
