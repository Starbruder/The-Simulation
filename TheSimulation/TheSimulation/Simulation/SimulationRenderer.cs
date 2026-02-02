using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheSimulation;

/// <summary>
/// Kümmert sich ausschließlich um die visuelle Darstellung der Simulation.
/// Trennt WPF-UI-Logik von der Simulations-Logik.
/// </summary>
public sealed class SimulationRenderer
{
    private readonly Canvas forestCanvas;

    private readonly SimulationConfig simulationConfig;
    private readonly RandomHelper randomHelper;

    private readonly ParticleGenerator particleGenerator;
    private readonly WindCompassVisualizer windVisualizer;

    private readonly Dictionary<Cell, Shape> treeShapes = [];
    private readonly Dictionary<Cell, FireAnimation> fireAnimations = [];

    private readonly Rectangle screenFlash;

    public SimulationRenderer(
        Canvas forestCanvas,
        SimulationConfig simulationConfig,
        RandomHelper randomHelper)
    {
        this.forestCanvas = forestCanvas;

        this.simulationConfig = simulationConfig;
        this.randomHelper = randomHelper;

        particleGenerator = new ParticleGenerator(forestCanvas);

        var windConfig = simulationConfig.EnvironmentConfig.WindConfig;
        var windHelper = new WindHelper(windConfig);
        windVisualizer = new WindCompassVisualizer(forestCanvas, windConfig, windHelper);

        if (simulationConfig.VisualEffectsConfig.ShowBoltScreenFlash)
        {
            InitializeScreenFlash();
        }
    }

    private void InitializeScreenFlash()
    {
        screenFlash.Width = forestCanvas.ActualWidth;
        screenFlash.Height = forestCanvas.ActualHeight;
        screenFlash.Fill = ColorsData.FlashColor;
        screenFlash.Opacity = 0;

        Panel.SetZIndex(screenFlash, int.MaxValue);
        forestCanvas.Children.Add(screenFlash);
    }

    public void DrawTree(Cell cell, Brush color, double size, TreeShapeType shapeType)
    {
        // Falls da schon was war: Sichergehen, dass es weg ist
        RemoveTree(cell);

        Shape shape = shapeType switch
        {
            TreeShapeType.Ellipse => new Ellipse(),
            TreeShapeType.Rectangle => new Rectangle(),
            _ => new Rectangle()
        };

        shape.Width = size;
        shape.Height = size;
        shape.Fill = color;
        shape.Tag = cell; // Nützlich für Debugging

        // Positionieren
        Canvas.SetLeft(shape, cell.X * size);
        Canvas.SetTop(shape, cell.Y * size);

        forestCanvas.Children.Add(shape);
        treeShapes[cell] = shape;
    }

    public void UpdateTreeColor(Cell cell, Brush color)
    {
        if (treeShapes.TryGetValue(cell, out var shape))
        {
            shape.Fill = color;
        }
    }

    public void RemoveTree(Cell cell)
    {
        if (treeShapes.Remove(cell, out var shape))
        {
            forestCanvas.Children.Remove(shape);
        }
    }

    public void StartFireAnimation(Cell burningCell, SimulationConfig config)
    {
        if (config.VisualEffectsConfig.ShowFlameAnimations)
        {
            // Verhindern von doppelten Animationen
            //if (_fireAnimations.ContainsKey(cell)) return;

            var fire = new FireAnimation(
                burningCell,
                forestCanvas,
                simulationConfig.TreeConfig.Size
            );

            fireAnimations[burningCell] = fire;
            fire.Start();
        }

        if (config.VisualEffectsConfig.ShowFireParticles
            && randomHelper.NextDouble() < 0.7)
        {
            particleGenerator.AddFireParticle(burningCell, config);
        }

        if (config.VisualEffectsConfig.ShowSmokeParticles)
        {
            particleGenerator.AddSmoke(burningCell, config);
        }
    }

    public void StopFireAnimation(Cell cell)
    {
        if (fireAnimations.Remove(cell, out var fire))
        {
            fire.Stop();
        }
    }

    public void UpdateWindVisualizer(Vector windVector)
    {
        windVisualizer.Update(windVector);
        DrawWindVisualizer();
    }

    public void DrawWindVisualizer()
        => windVisualizer.Draw();

    public async Task ShowLightningEffectAsync(Cell target)
    {
        var frozenLightningBrush = ColorsData.LightningColor;
        var lightningCell = CreateCellShape(target, frozenLightningBrush);

        Canvas.SetLeft(lightningCell, target.X * simulationConfig.TreeConfig.Size);
        Canvas.SetTop(lightningCell, target.Y * simulationConfig.TreeConfig.Size);
        forestCanvas.Children.Add(lightningCell);

        var boltEffect = CreateLightningBolt(target);
        forestCanvas.Children.Add(boltEffect);

        if (simulationConfig.VisualEffectsConfig.ShowLightning)
        {
            await FlashScreen();
        }
        await Task.Delay(millisecondsDelay: 80);

        forestCanvas.Children.Remove(lightningCell);
        forestCanvas.Children.Remove(boltEffect);
    }

    private Polyline CreateLightningBolt(Cell target)
    {
        var size = simulationConfig.TreeConfig.Size;

        var startX = target.X * size + size / 2f;
        var startY = 0f;

        var endX = startX;
        var endY = target.Y * size + size / 2f;

        var points = new PointCollection
        {
            new(startX, startY)
        };

        const byte boltSegments = 8;
        for (var i = 1; i < boltSegments; i++)
        {
            var t = i / (float)boltSegments;
            var x = startX + randomHelper.NextDouble(-15, 15);
            var y = startY + (endY - startY) * t;
            points.Add(new(x, y));
        }

        points.Add(new(endX, endY));

        var frozenLightningBrush = ColorsData.LightningColor;

        return new Polyline
        {
            Points = points,
            Stroke = frozenLightningBrush,
            StrokeThickness = 2.5,
            Opacity = 1
        };
    }

    private Shape CreateCellShape(Cell cell, Brush color)
    {
        var size = simulationConfig.TreeConfig.Size;

        Shape shape = simulationConfig.VisualEffectsConfig.TreeShape switch
        {
            TreeShapeType.Ellipse => new Ellipse(),
            TreeShapeType.Rectangle => new Rectangle(),
            _ => throw new NotSupportedException(
                $"Shape {simulationConfig.VisualEffectsConfig.TreeShape} is not supported."
            )
        };

        if (!color.IsFrozen)
        {
            color.Freeze();
        }

        shape.Width = size;
        shape.Height = size;
        shape.Fill = color;
        shape.Tag = cell;

        return shape;
    }

    /// <summary>
    /// Briefly flashes the screen by temporarily increasing the opacity of the screen overlay for ~1 Frame.
    /// </summary>
    /// <remarks>This method is intended to provide a quick visual feedback effect. It should be awaited to
    /// ensure the flash completes before proceeding with subsequent UI updates.</remarks>
    /// <returns>A task that represents the asynchronous flash operation.</returns>
    private async Task FlashScreen()
    {
        screenFlash.Opacity = 0.6;
        await Task.Delay(40);
        screenFlash.Opacity = 0;
    }

    /// <summary>
    /// Räumt eine Zelle visuell komplett auf (für HardReset).
    /// RemoveTree() und StopFireAnimation() zugleich.
    /// </summary>
    public void ClearCell(Cell cell)
    {
        RemoveTree(cell);
        StopFireAnimation(cell);
    }

    public void Dispose()
    {
        forestCanvas.Children.Clear();
        treeShapes.Clear();

        foreach (var fireAnimation in fireAnimations.Values)
        {
            fireAnimation.Stop();
        }
        fireAnimations.Clear();
    }
}
