using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheSimulation;

/// <summary>
/// Verantwortlich für die gesamte visuelle Darstellung der Simulation auf einem WPF-Canvas.
/// Diese Klasse entkoppelt die mathematische Simulationslogik von der Grafik-Engine.
/// </summary>
/// <remarks>
/// Der Renderer verwaltet Lebenszyklen von UI-Elementen (Bäume, Feuer, Partikel) und optimiert
/// den Zugriff über Dictionaries, um teure Suchvorgänge im Visual Tree des Canvas zu vermeiden.
/// </remarks>
public sealed class SimulationRenderer
{
    private readonly Canvas forestCanvas;
    private readonly SimulationConfig simulationConfig;
    private readonly RandomHelper randomHelper;

    private readonly ParticleGenerator particleGenerator;
    private readonly WindCompassVisualizer windVisualizer;

    /// <summary>Ordnet jeder Simulationszelle das entsprechende grafische Shape-Objekt zu.</summary>
    private readonly Dictionary<Cell, Shape> treeShapes = [];

    /// <summary>Verwaltet aktive Feueranimationen für brennende Zellen.</summary>
    private readonly Dictionary<Cell, FireAnimation> fireAnimations = [];

    /// <summary>Ein Rechteck, das den gesamten Canvas abdeckt, um Blitzeffekte (Screen Flash) zu simulieren.</summary>
    private readonly Rectangle screenFlash = new();

    /// <summary>
    /// Initialisiert eine neue Instanz des Renderers und bereitet Hilfssysteme wie Partikel und Wind vor.
    /// </summary>
    /// <param name="forestCanvas">Die WPF-Zeichenfläche.</param>
    /// <param name="simulationConfig">Die Konfiguration der aktuellen Simulation.</param>
    /// <param name="randomHelper">Ein Hilfsobjekt für zufallsbasierte visuelle Variationen.</param>
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
            if (forestCanvas.IsLoaded)
            {
                InitializeScreenFlash();
            }
            else
            {
                forestCanvas.Loaded += OnCanvasLoaded;
            }
        }
    }

    /// <summary>
    /// Event-Handler, der aufgerufen wird, sobald der Canvas im UI-Tree geladen wurde.
    /// </summary>
    private void OnCanvasLoaded(object sender, RoutedEventArgs e)
    {
        // Event sofort wieder abhängen (Clean-up)
        forestCanvas.Loaded -= OnCanvasLoaded;
        InitializeScreenFlash();
    }

    /// <summary>
    /// Erstellt das Overlay für den Bildschrim-Blitzeffekt und fügt es dem Canvas hinzu.
    /// </summary>
    private void InitializeScreenFlash()
    {
        // Jetzt hat ActualWidth/Height garantiert den richtigen Wert (> 0)
        screenFlash.Width = forestCanvas.ActualWidth;
        screenFlash.Height = forestCanvas.ActualHeight;
        screenFlash.Fill = ColorsData.FlashColor;
        screenFlash.Opacity = 0;

        // Verhindert, dass der Blitz Mausklicks abfängt
        screenFlash.IsHitTestVisible = false;

        Panel.SetZIndex(screenFlash, int.MaxValue);

        // Wir prüfen, ob es schon im Visual Tree ist, um doppeltes Hinzufügen zu vermeiden
        if (!forestCanvas.Children.Contains(screenFlash))
        {
            forestCanvas.Children.Add(screenFlash);
        }
    }

    /// <summary>
    /// Zeichnet einen Baum an der Position der angegebenen Zelle.
    /// </summary>
    /// <param name="cell">Die logische Zelle in der Simulationsmatrix.</param>
    /// <param name="color">Die Farbe des Baumes (z.B. Grün für lebend, Grau für verbrannt).</param>
    /// <param name="size">Die Kantenlänge bzw. der Durchmesser des Objekts.</param>
    /// <param name="shapeType">Die geometrische Form des Baumes.</param>
    public void DrawTree(Cell cell, Brush color, double size, TreeShapeType shapeType)
    {
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
        shape.Tag = cell; // Verknüpfung für Debug-Zwecke (Hit-Testing)

        Canvas.SetLeft(shape, cell.X * size);
        Canvas.SetTop(shape, cell.Y * size);

        forestCanvas.Children.Add(shape);
        treeShapes[cell] = shape;
    }

    /// <summary>
    /// Aktualisiert die Farbe eines bestehenden Baumes, ohne das Objekt neu zu erstellen (Performance-Optimierung).
    /// </summary>
    public void UpdateTreeColor(Cell cell, Brush color)
    {
        if (treeShapes.TryGetValue(cell, out var shape))
        {
            shape.Fill = color;
        }
    }

    /// <summary>
    /// Entfernt die grafische Darstellung eines Baumes vom Canvas.
    /// </summary>
    public void RemoveTree(Cell cell)
    {
        if (treeShapes.Remove(cell, out var shape))
        {
            forestCanvas.Children.Remove(shape);
        }
    }

    /// <summary>
    /// Startet visuelle Effekte für eine brennende Zelle, inklusive Flammen-Animationen und Partikelsystemen.
    /// </summary>
    /// <param name="burningCell">Die Zelle, die in Flammen steht.</param>
    /// <param name="config">Die Konfiguration, um zu prüfen, welche Effekte aktiviert sind.</param>
    public void StartFireAnimation(Cell burningCell, SimulationConfig config)
    {
        if (config.VisualEffectsConfig.ShowFlameAnimations)
        {
            var fire = new FireAnimation(
                burningCell,
                forestCanvas,
                simulationConfig.TreeConfig.Size
            );

            fireAnimations[burningCell] = fire;
            fire.Start();
        }

        // Partikel (Funken/Asche) werden mit einer gewissen Zufälligkeit erzeugt
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

    /// <summary>
    /// Stoppt die Flammen-Animation für eine Zelle (z.B. wenn der Baum abgebrannt oder gelöscht ist).
    /// </summary>
    public void StopFireAnimation(Cell cell)
    {
        if (fireAnimations.Remove(cell, out var fire))
        {
            fire.Stop();
        }
    }

    /// <summary>
    /// Aktualisiert die Windrichtung und -stärke im visuellen Kompass.
    /// </summary>
    public void UpdateWindVisualizer(Vector windVector)
    {
        windVisualizer.Update(windVector);
        DrawWindVisualizer();
    }

    /// <summary>Löst den Zeichenvorgang des Windkompasses aus.</summary>
    public void DrawWindVisualizer()
        => windVisualizer.Draw();

    /// <summary>
    /// Führt einen vollständigen Blitzeinschlag-Effekt aus. 
    /// Zeichnet einen Blitz-Zickzack, färbt die Zielzelle kurzzeitig weiß und löst einen Screen-Flash aus.
    /// </summary>
    /// <param name="target">Die Zelle, in die der Blitz einschlägt.</param>
    public async Task ShowLightningEffectAsync(Cell target)
    {
        var frozenLightningBrush = ColorsData.LightningColor;
        var lightningCell = CreateCellShape(target, frozenLightningBrush);

        Canvas.SetLeft(lightningCell, target.X * simulationConfig.TreeConfig.Size);
        Canvas.SetTop(lightningCell, target.Y * simulationConfig.TreeConfig.Size);
        forestCanvas.Children.Add(lightningCell);

        var boltEffect = CreateLightningBolt(target);
        forestCanvas.Children.Add(boltEffect);

        if (simulationConfig.VisualEffectsConfig.ShowBoltScreenFlash)
        {
            await FlashScreen();
        }

        // Der Blitz bleibt nur für einen kurzen Augenblick sichtbar
        await Task.Delay(millisecondsDelay: 80);

        forestCanvas.Children.Remove(lightningCell);
        forestCanvas.Children.Remove(boltEffect);
    }

    /// <summary>
    /// Erzeugt eine geometrische Linie (Polyline), die einen gezackten Blitzstrahl von "Himmel" zum Baum darstellt.
    /// </summary>
    private Polyline CreateLightningBolt(Cell target)
    {
        var size = simulationConfig.TreeConfig.Size;
        var startX = target.X * size + size / 2f;
        var startY = 0f;
        var endX = startX;
        var endY = target.Y * size + size / 2f;

        var points = new PointCollection { new(startX, startY) };

        const byte boltSegments = 8;
        for (var i = 1; i < boltSegments; i++)
        {
            var t = i / (float)boltSegments;
            var x = startX + randomHelper.NextDouble(-15, 15); // Horizontale Abweichung für den Zickzack-Look
            var y = startY + (endY - startY) * t;
            points.Add(new(x, y));
        }

        points.Add(new(endX, endY));

        return new Polyline
        {
            Points = points,
            Stroke = ColorsData.LightningColor,
            StrokeThickness = 2.5,
            Opacity = 1
        };
    }

    /// <summary>
    /// Hilfsmethode zur Erstellung einer geometrischen Form für eine bestimmte Zelle.
    /// </summary>
    private Shape CreateCellShape(Cell cell, Brush color)
    {
        var size = simulationConfig.TreeConfig.Size;

        Shape shape = simulationConfig.VisualEffectsConfig.TreeShape switch
        {
            TreeShapeType.Ellipse => new Ellipse(),
            TreeShapeType.Rectangle => new Rectangle(),
            _ => throw new NotSupportedException($"Shape {simulationConfig.VisualEffectsConfig.TreeShape} is not supported.")
        };

        // Einfrieren der Brushes verbessert die Performance massiv (Read-Only Status für den Render-Thread)
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
    /// Lässt den Bildschirm kurzzeitig weiß aufleuchten (Flash-Effekt).
    /// </summary>
    private async Task FlashScreen()
    {
        screenFlash.Opacity = 0.6;
        await Task.Delay(40);
        screenFlash.Opacity = 0;
    }

    /// <summary>
    /// Entfernt sowohl den Baum als auch alle aktiven Feueranimationen einer Zelle.
    /// </summary>
    public void ClearCell(Cell cell)
    {
        RemoveTree(cell);
        StopFireAnimation(cell);
    }

    /// <summary>
    /// Stoppt alle laufenden Animationen und leert den Canvas sowie alle internen Caches.
    /// </summary>
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
