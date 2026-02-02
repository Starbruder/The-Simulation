using System.Runtime.CompilerServices;
using System.Windows;

namespace TheSimulation;

/// <summary>
/// Hilfsklasse zur Berechnung von Windeffekten und zur Verwaltung dynamischer Windänderungen.
/// Berechnet, wie stark der Wind die Brandausbreitung zwischen zwei Zellen beeinflusst.
/// </summary>
/// <param name="config">Die zugrunde liegende Windkonfiguration.</param>
public sealed class WindHelper(WindConfig config)
{
    /// <summary>
    /// Alles was mit Wind zu tun hat: Richtung, Stärke, Zufälligkeit.
    /// </summary>
    private readonly WindConfig config = config;
    private readonly RandomHelper randomHelper = new();

    /// <summary>
    /// Ruft den aktuellen Windwinkel in Grad ab (0-360°).
    /// </summary>
    public double CurrentWindAngleDegrees { get; private set; } = (int)config.Direction;

    /// <summary>
    /// Ruft die aktuelle Windstärke ab (typischerweise im Bereich 0 bis 1).
    /// </summary>
    public double CurrentWindStrength { get; private set; } = config.Strength;

    /// <summary>
    /// Berechnet den Multiplikator für die Brandausbreitung basierend auf der Windrichtung und -stärke.
    /// Nutzt das Skalarprodukt zwischen Windvektor und Ausbreitungsrichtung.
    /// </summary>
    /// <param name="from">Die Zelle, in der das Feuer aktuell brennt.</param>
    /// <param name="to">Die Nachbarzelle, auf die das Feuer übergreifen könnte.</param>
    /// <returns>Ein Faktor (>= 0.1), mit dem die Basis-Ausbreitungswahrscheinlichkeit multipliziert wird.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double CalculateWindEffect(Cell from, Cell to)
    {
        // 1. Schritt: Wo will das Feuer hin?
        // Wir berechnen den Unterschied zwischen der aktuellen Position (from) 
        // und dem Ziel (to) in X- und Y-Richtung.
        var diffrenceX = to.X - from.X;
        var diffrenceY = to.Y - from.Y;

        // 2. Schritt: Die Richtung festlegen
        // Wir erstellen einen "Pfeil" (Vektor), der genau zum Ziel zeigt.
        var spreadDirection = new Vector(diffrenceX, diffrenceY);
        // 'Normalize' macht den Pfeil genau 1 Einheit lang. Uns interessiert nur 
        // die Richtung, nicht wie weit die Bäume weg sind.
        spreadDirection.Normalize();

        // 3. Schritt: Was macht der Wind?
        var windVector = GetWindVector();
        // Wenn gar kein Wind weht (Stärke 0), bleibt alles beim Alten (Faktor 1).
        if (windVector.Length == 0)
        {
            return 1;
        }

        // Auch den Wind-Pfeil bringen wir auf die Standardlänge 1.
        windVector.Normalize();

        // 4. Schritt: Der "Vergleich" (Das Skalarprodukt)
        // Das ist der wichtigste Teil. Wir vergleichen, wie gut die beiden Pfeile 
        // (Feuer-Richtung und Wind-Richtung) zusammenpassen:
        // alignment = 1  -> Wind bläst exakt in Richtung des nächsten Baums (Rückenwind).
        // alignment = 0  -> Wind bläst seitlich am Baum vorbei (kein großer Effekt).
        // alignment = -1 -> Wind bläst dem Feuer direkt entgegen (Gegenwind).
        var alignment = Vector.Multiply(spreadDirection, windVector);

        // 5. Schritt: Den Bonus oder Malus berechnen
        // Wir nehmen die Windstärke (z.B. 0.5 für mäßigen Wind) und verrechnen sie.
        // Bei Rückenwind: 1 + (0.5 * 1) = 1.5 (50% höhere Chance).
        // Bei Gegenwind: 1 + (0.5 * -1) = 0.5 (50% geringere Chance).
        var effect = 1 + CurrentWindStrength * alignment;

        // 6. Schritt: Sicherheitscheck
        // Das Feuer soll niemals rückwärts brennen oder unmöglich werden. 
        // Selbst bei heftigstem Gegenwind bleibt eine kleine Restchance von 0.1 (10%).
        return Math.Max(0.1, effect);
    }

    /// <summary>
    /// Erzeugt den aktuellen Windvektor basierend auf der statischen Richtung oder dem dynamischen Winkel.
    /// </summary>
    /// <returns>Ein <see cref="Vector"/>, der die Richtung und Stärke des Windes repräsentiert.</returns>
    public Vector GetWindVector()
    {
        if (config.RandomDirection)
        {
            return WindMapper.ConvertWindAngleDegreesToVector(
                CurrentWindAngleDegrees, CurrentWindStrength);
        }

        return WindMapper.GetWindVector(config.Direction);
    }

    /// <summary>
    /// Aktualisiert die Windparameter (Richtung und Stärke) durch kleine Zufallsvariationen (Fluktuation),
    /// sofern dies in der Konfiguration aktiviert ist.
    /// </summary>
    public void RandomizeAndUpdateWind()
    {
        if (config.RandomDirection)
        {
            // Windrichtung um ±5° ändern
            RandomizeWindDirection(fluctuation: 5);
        }

        if (config.RandomStrength)
        {
            // Windstärke um ±3% ändern
            RandomizeWindStrengh(fluctuation: 0.03f);
        }
    }

    /// <summary>
    /// Variiert den aktuellen Windwinkel zufällig innerhalb eines Bereichs.
    /// </summary>
    /// <param name="fluctuation">Die maximale Abweichung in Grad.</param>
    private void RandomizeWindDirection(uint fluctuation)
    {
        var deltaDirection = (randomHelper.NextDouble() * 2 - 1) * fluctuation;
        CurrentWindAngleDegrees = (CurrentWindAngleDegrees + deltaDirection + 360) % 360;
    }

    /// <summary>
    /// Variiert die Windstärke zufällig innerhalb eines Bereichs und klammert das Ergebnis auf ein Maximum.
    /// </summary>
    /// <param name="fluctuation">Die maximale prozentuale Abweichung.</param>
    private void RandomizeWindStrengh(float fluctuation)
    {
        var deltaStrength = randomHelper.NextDouble(-fluctuation, fluctuation);
        CurrentWindStrength += deltaStrength;

        // Sicherstellen, dass die Windstärke im erlaubten Bereich bleibt
        const uint MaxWindStrength = 1;
        CurrentWindStrength = Math.Clamp(CurrentWindStrength, 0, MaxWindStrength);
    }
}
