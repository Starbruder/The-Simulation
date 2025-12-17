using System.Windows;
using System.Windows.Shapes;

namespace TheSimulation;

public sealed class Particle(Ellipse visual, Vector velocity, float lifetime)
{
    public Ellipse Visual { get; } = visual;
    public Vector Velocity { get; } = velocity;
    public float Lifetime { get; private set; } = lifetime;

    public void DecreaseLifetime(float amount)
    {
        Lifetime -= amount;
    }
}
