using System.Windows;

namespace TheSimulation;

public sealed class WindHelper(SimulationConfig simulationConfig)
{
    public double CalculateWindEffect(Cell from, Cell to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        // Richtungsvektor der Feuerausbreitung
        var spreadDir = new Vector(dx, dy);
        spreadDir.Normalize();

        // Wind normalisieren
        var windDir = simulationConfig.WindConfig.Direction;
        var windVector = WindMapper.GetWindVector(windDir);
        if (windVector.Length == 0)
        {
            return 1.0;
        }

        windVector.Normalize();

        // Skalarprodukt: -1 .. 1
        var alignment = Vector.Multiply(spreadDir, windVector);

        // Gegenwind soll stark bremsen
        var effect = 1 + simulationConfig.WindConfig.Strength * alignment;

        // Keine negativen Wahrscheinlichkeiten
        return Math.Max(0.1, effect);
    }
}
