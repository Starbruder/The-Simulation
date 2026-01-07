using System.Windows;
using System.Windows.Shapes;

namespace TheSimulation;

/// <summary>
/// <see langword="public"/> <see langword="sealed"/> record representing a particle in the simulation.
/// </summary>
/// <param name="Visual"></param>
/// <param name="Velocity"></param>
/// <param name="Lifetime"></param>
public sealed record Particle(Ellipse Visual, Vector Velocity, double Lifetime)
{
    public double Lifetime { get; private set; } = Lifetime;

    public void DecreaseLifetime(double amount)
    {
        Lifetime -= amount;
    }
}
