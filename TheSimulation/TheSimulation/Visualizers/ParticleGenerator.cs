using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TheSimulation;

public sealed class ParticleGenerator
{
    private readonly Canvas canvas;
    private readonly List<Particle> particles = [];
    private readonly DispatcherTimer updateTimer;
    private readonly RandomHelper randomHelper = new();

    public static readonly Brush[] FireColors =
    [
        Brushes.OrangeRed,
        Brushes.DarkOrange,
        Brushes.Red,
        Brushes.Firebrick
    ];

    public ParticleGenerator(Canvas canvas, uint updateIntervalMs = 30) // ~30 FPS
    {
        this.canvas = canvas;

        updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(updateIntervalMs)
        };
        updateTimer.Tick += (_, _) => UpdateParticles();
        updateTimer.Start();
    }

    public void SpawnParticle(Point position, Brush color, double size = 3, double lifetime = 1)
    {
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = color,
            Opacity = 1
        };

        Canvas.SetLeft(ellipse, position.X);
        Canvas.SetTop(ellipse, position.Y);
        canvas.Children.Add(ellipse);

        // zufällige Bewegung
        var velocity = new Vector(randomHelper.NextDouble() * 2 - 1, -(randomHelper.NextDouble() * 2)); // nach oben

        particles.Add(new(ellipse, velocity, lifetime));
    }

    public void AddFireParticle(Cell cell, SimulationConfig simulationConfig)
    {
        var pos = new Point(
        cell.X * simulationConfig.TreeConfig.Size,
        cell.Y * simulationConfig.TreeConfig.Size);

        var fireColors = FireColors;
        var color = fireColors[randomHelper.NextInt(0, fireColors.Length)];

        SpawnParticle(
            pos,
            color,
            size: 2 + randomHelper.NextInt(0, 3),
            lifetime: 0.6 + randomHelper.NextDouble() * 0.5
        );
    }

    public void AddSmoke(Cell cell, SimulationConfig config)
    {
        var pos = new Point(
            cell.X * config.TreeConfig.Size,
            cell.Y * config.TreeConfig.Size);

        SpawnParticle(
            pos,
            Brushes.Gray,
            size: 5 + randomHelper.NextInt(0, 4),
            lifetime: 1.2 + randomHelper.NextDouble()
        );
    }

    private void UpdateParticles()
    {
        for (var i = particles.Count - 1; i >= 0; i--)
        {
            var p = particles[i];
            p.DecreaseLifetime(0.05f);
            if (p.Lifetime <= 0)
            {
                canvas.Children.Remove(p.Visual);
                particles.RemoveAt(i);
                continue;
            }

            Canvas.SetLeft(p.Visual, Canvas.GetLeft(p.Visual) + p.Velocity.X);
            Canvas.SetTop(p.Visual, Canvas.GetTop(p.Visual) + p.Velocity.Y);
            p.Visual.Opacity = p.Lifetime;
        }
    }
}
