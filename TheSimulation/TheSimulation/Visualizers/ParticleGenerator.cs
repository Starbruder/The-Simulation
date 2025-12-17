using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;

namespace TheSimulation;

public sealed class ParticleGenerator
{
	private readonly Canvas canvas;
	private readonly List<Particle> particles = [];
	private readonly DispatcherTimer updateTimer;
	private readonly Random random = new();

	public static readonly Brush[] FireColors =
	[
		Brushes.OrangeRed,
		Brushes.DarkOrange,
		Brushes.Red,
		Brushes.Firebrick
	];

	public ParticleGenerator(Canvas canvas, int updateIntervalMs = 33) // ~30 FPS
	{
		this.canvas = canvas;

		updateTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(updateIntervalMs)
		};
		updateTimer.Tick += (_, _) => UpdateParticles();
		updateTimer.Start();
	}

	public void SpawnParticle(Point position, Brush color, double size = 3, float lifetime = 1f)
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
		var velocity = new Vector(random.NextDouble() * 2 - 1, -(random.NextDouble() * 2)); // nach oben

		particles.Add(new Particle(ellipse, velocity, lifetime));
	}

	private void UpdateParticles()
	{
		for (int i = particles.Count - 1; i >= 0; i--)
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
