using System.Windows;
using System.Windows.Shapes;

namespace TheSimulation;

public sealed class Particle(Ellipse visual, Vector velocity, double lifetime)
{
    public Ellipse Visual { get; } = visual;
    public Vector Velocity { get; } = velocity;
    public double Lifetime { get; private set; } = lifetime;

    public void DecreaseLifetime(double amount)
    {
        Lifetime -= amount;
    }
}
