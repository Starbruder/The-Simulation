using System.Windows;

namespace TheSimulation;

public sealed class WindHelper(SimulationConfig simulationConfig)
{
    private readonly SimulationConfig config = simulationConfig;
    private readonly RandomHelper randomHelper = new();

    public double CurrentWindAngleDegrees { get; private set; } = (int)simulationConfig.EnvironmentConfig.WindConfig.Direction;

    public double CurrentWindStrength { get; private set; } = simulationConfig.EnvironmentConfig.WindConfig.Strength;

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
        var effect = 1 + CurrentWindStrength * alignment;

        // Keine negativen Wahrscheinlichkeiten
        return Math.Max(0.1, effect);
    }

    public Vector GetWindVector()
    {
        if (!config.EnvironmentConfig.WindConfig.RandomDirection)
        {
            return WindMapper.GetWindVector(config.EnvironmentConfig.WindConfig.Direction);
        }

        // in Radiant umrechnen
        var rad = CurrentWindAngleDegrees * Math.PI / 180;
        var x = Math.Cos(rad);
        var y = Math.Sin(rad);

        var vector = new Vector(x, y);
        vector.Normalize();
        vector *= CurrentWindStrength;

        return vector;
    }

    public void RandomizeAndUpdateWind()
    {
        if (config.EnvironmentConfig.WindConfig.RandomDirection)
        {
            // Windrichtung um ±5° ändern
            RandomizeWindDirection(fluctuation: 5);
        }

        if (config.EnvironmentConfig.WindConfig.RandomStrength)
        {
            // Windstärke um ±3% ändern
            RandomizeWindStrengh(fluctuation: 0.03f);
        }
    }

    private void RandomizeWindDirection(uint fluctuation)
    {
        var deltaDirection = (randomHelper.NextDouble() * 2 - 1) * fluctuation;
        CurrentWindAngleDegrees = (CurrentWindAngleDegrees + deltaDirection + 360) % 360;
    }

    private void RandomizeWindStrengh(float fluctuation)
    {
        var deltaStrength = randomHelper.NextDouble(-fluctuation, fluctuation);
        CurrentWindStrength += deltaStrength;

        // Sicherstellen, dass die Windstärke im erlaubten Bereich bleibt
        const uint MaxWindStrength = 1;
        CurrentWindStrength = Math.Clamp(CurrentWindStrength, 0, MaxWindStrength);
    }
}
