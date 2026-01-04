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

        var spreadDir = new Vector(dx, dy);
        spreadDir.Normalize();

        var windVector = GetWindVector();
        if (windVector.Length == 0)
        {
            return 1.0;
        }

        windVector.Normalize();

        var alignment = Vector.Multiply(spreadDir, windVector);

        var effect = 1 + alignment; // optional: multipliziere mit Stärke, wenn nicht schon im Vector

        return Math.Max(0.1, effect);
    }

    public Vector GetWindVector()
    {
        // in Radiant umrechnen
        var rad = CurrentWindAngleDegrees * Math.PI / 180.0;
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
        var delta = (randomHelper.NextDouble() * 2 - 1) * 5.0;
        CurrentWindAngleDegrees = (CurrentWindAngleDegrees + delta + 360) % 360;
    }
}
