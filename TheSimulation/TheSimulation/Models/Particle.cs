using System.Windows;
using System.Windows.Shapes;

namespace TheSimulation;

public sealed record Particle(Ellipse Visual, Vector Velocity, double Lifetime)
{
    public double Lifetime { get; private set; } = Lifetime;

    public void DecreaseLifetime(double amount)
    {
        Lifetime -= amount;
    }
}
