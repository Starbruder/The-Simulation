using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TheSimulation;

public sealed class FireAnimation
{
    private readonly Cell burningCell;
    private readonly Canvas canvas;
    private readonly double cellSize;

    private readonly DispatcherTimer timer;
    private readonly List<Ellipse> flames = [];

    public FireAnimation(Cell burningCell, Canvas canvas, double cellSize)
    {
        this.burningCell = burningCell;
        this.canvas = canvas;
        this.cellSize = cellSize;

        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(80)
        };
        timer.Tick += (_, _) => Animate();
    }

    public void Start() => timer.Start();

    public void Stop()
    {
        timer.Stop();
        foreach (var flame in flames)
        {
            canvas.Children.Remove(flame);
        }

        flames.Clear();
    }

    private void Animate()
    {
        foreach (var flame in flames)
        {
            canvas.Children.Remove(flame);
        }

        flames.Clear();

        SpawnFlames();
    }

    private void SpawnFlames()
    {
        var centerX = burningCell.X * cellSize + cellSize / 2;
        var centerY = burningCell.Y * cellSize + cellSize / 2;

        var rand = Random.Shared;

        const int flameCount = 1;

        for (var i = 0; i < flameCount; i++)
        {
            var width = rand.NextDouble() * 4 + 4;
            var height = rand.NextDouble() * 8 + 8;

            var flame = new Ellipse
            {
                Width = width,
                Height = height,
                Fill = Brushes.OrangeRed,
                Opacity = 0.85,
                IsHitTestVisible = false
            };

            // 🔥 leichte Zufälligkeit + nach oben versetzt
            Canvas.SetLeft(flame, centerX - width / 2 + rand.NextDouble() * 6 - 3);
            Canvas.SetTop(flame, centerY - height - rand.NextDouble() * 4);

            Panel.SetZIndex(flame, 1000); // 🔝 über Bäumen

            canvas.Children.Add(flame);
            flames.Add(flame);
        }
    }
}

