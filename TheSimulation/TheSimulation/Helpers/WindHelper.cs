using System.Windows;

namespace TheSimulation;

public sealed class WindHelper(SimulationConfig simulationConfig)
{
    private readonly SimulationConfig config = simulationConfig;
    private readonly RandomHelper randomHelper = new();

    public double CurrentWindAngleDegrees { get; private set; } = (int)simulationConfig.WindConfig.Direction;

    public double CalculateWindEffect(Cell from, Cell to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        // Richtungsvektor der Feuerausbreitung
        var spreadDir = new Vector(dx, dy);
        spreadDir.Normalize();

        // Wind normalisieren
        var windVector = GetWindVector();
        if (windVector.Length == 0)
        {
            return 1;
        }

        windVector.Normalize();

        // Skalarprodukt: -1 .. 1
        var alignment = Vector.Multiply(spreadDir, windVector);

        // multipliziere mit Stärke
        var effect = 1 + simulationConfig.WindConfig.Strength * alignment;

        // Keine negativen Wahrscheinlichkeiten
        return Math.Max(0.1, effect);
    }

    public Vector GetWindVector()
    {
        if (!config.WindConfig.RandomDirection)
        {
            return WindMapper.GetWindVector(config.WindConfig.Direction);
        }

        // in Radiant umrechnen
        var rad = CurrentWindAngleDegrees * Math.PI / 180;
        var x = Math.Cos(rad);
        var y = Math.Sin(rad);

        var vector = new Vector(x, y);
        vector.Normalize();
        vector *= config.WindConfig.Strength;

        return vector;
    }

    public void RandomizedAndUpdateWindDirection()
    {
        // z. B. ±5° pro Tick
        var delta = (randomHelper.NextDouble() * 2 - 1) * 5;
        CurrentWindAngleDegrees = (CurrentWindAngleDegrees + delta + 360) % 360;
    }
}
