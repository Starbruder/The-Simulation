using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TheSimulation;

/// <summary>
/// Generates and manages visual particle effects, such as fire and smoke, on a specified WPF canvas.
/// </summary>
/// <remarks>
/// The ParticleGenerator is designed for use in graphical simulations or visualizations that require dynamic particle effects.
/// It automatically updates and animates particles at a fixed interval, handling their creation, movement, and removal.
/// This class is not thread-safe and should be used on the UI thread associated with the provided canvas.
/// </remarks>
public sealed class ParticleGenerator
{
    private readonly Canvas canvas;
    private readonly List<Particle> particles = [];
    private readonly DispatcherTimer updateTimer;
    private readonly RandomHelper randomHelper = new();

    /// <summary>
    /// Initializes a new instance of the ParticleGenerator class that renders animated particles on the specified canvas at a given update interval.
    /// </summary>
    /// <remarks>
    /// A lower update interval results in smoother animations but may increase CPU usage. 
    /// The generator starts updating particles immediately upon construction.
    /// </remarks>
    /// <param name="canvas">
    /// The Canvas control on which particles will be rendered. Cannot be null.
    /// </param>
    /// <param name="updateIntervalMs">
    /// The interval, in milliseconds, between particle updates.
    /// Must be greater than zero. Defaults to 30 milliseconds.
    /// </param>
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

    /// <summary>
    /// Spawns a particle at the specified position with the given color, size, and lifetime.
    /// </summary>
    /// <remarks>
    /// The particle is added to the canvas and will move with a random upward velocity. 
    /// This method is typically used to create visual effects such as bursts or trails.
    /// </remarks>
    /// <param name="position">
    /// The location at which to place the center of the particle.
    /// </param>
    /// <param name="color">
    /// The brush used to fill the particle's shape.
    /// </param>
    /// <param name="size">
    /// The diameter of the particle, in device-independent units. The default value is 3.
    /// </param>
    /// <param name="lifetime">
    /// The duration, in seconds, for which the particle remains visible.
    /// The default value is 1.
    /// </param>
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

    /// <summary>
    /// Adds a fire particle effect at the specified cell location using the provided simulation configuration.
    /// </summary>
    /// <param name="cell">
    /// The cell at which to spawn the fire particle.
    /// Must not be null.
    /// </param>
    /// <param name="simulationConfig">
    /// The simulation configuration that determines particle placement and appearance.
    /// Must not be null.
    /// </param>
    public void AddFireParticle(Cell cell, SimulationConfig simulationConfig)
    {
        var pos = new Point(
        cell.X * simulationConfig.TreeConfig.Size,
        cell.Y * simulationConfig.TreeConfig.Size);

        var frozenFireBrushes = ColorsData.FireColors;
        var frozenBrush = frozenFireBrushes[randomHelper.NextInt(0, frozenFireBrushes.Length)];

        SpawnParticle(
            pos,
            frozenBrush,
            size: 2 + randomHelper.NextInt(0, 3),
            lifetime: 0.6 + randomHelper.NextDouble() * 0.5
        );
    }

    /// <summary>
    /// Adds a smoke particle effect at the specified cell location using the provided simulation configuration.
    /// </summary>
    /// <param name="cell">
    /// The cell at which to add the smoke particle. Must not be null.
    /// </param>
    /// <param name="config">
    /// The simulation configuration that determines particle placement and size.
    /// Must not be null.
    /// </param>
    public void AddSmoke(Cell cell, SimulationConfig config)
    {
        var pos = new Point(
            cell.X * config.TreeConfig.Size,
            cell.Y * config.TreeConfig.Size);

        var frozenBurnedTreeBrush = ColorsData.BurnedTreeColor;

        SpawnParticle(
            pos,
            frozenBurnedTreeBrush,
            size: 5 + randomHelper.NextInt(0, 4),
            lifetime: 1.2 + randomHelper.NextDouble()
        );
    }

    /// <summary>
    /// Updates the state of all active particles, advancing their positions and removing any particles whose lifetimes have expired.
    /// </summary>
    /// <remarks>
    /// This method should be called regularly, such as within a rendering or update loop, to animate and manage the lifecycle of particles.
    /// Removed particles are also detached from the canvas to free resources.
    /// </remarks>
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
