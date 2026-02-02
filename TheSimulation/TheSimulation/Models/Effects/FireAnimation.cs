using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TheSimulation;

/// <summary>
/// Verwaltet die visuelle Animation eines Feuers auf einer brennenden Zelle.
/// Erzeugt dynamische Partikeleffekte (Flammen) mittels WPF-Shapes.
/// </summary>
/// <remarks>
/// Jede Instanz besitzt einen eigenen <see cref="DispatcherTimer"/>, der in kurzen Intervallen 
/// die Flammen-Grafiken aktualisiert, um ein Flackern zu simulieren.
/// </remarks>
public sealed class FireAnimation
{
    private readonly Cell burningCell;
    private readonly Canvas canvas;
    private readonly double cellSize;

    private readonly DispatcherTimer timer;
    private readonly List<Ellipse> flames = [];

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="FireAnimation"/>.
    /// </summary>
    /// <param name="burningCell">Die logische Zelle, auf der das Feuer dargestellt wird.</param>
    /// <param name="canvas">Das Ziel-Canvas, auf dem die Flammen gezeichnet werden.</param>
    /// <param name="cellSize">Die aktuelle Größe einer Zelle in Pixeln zur korrekten Positionierung.</param>
    public FireAnimation(Cell burningCell, Canvas canvas, double cellSize)
    {
        this.burningCell = burningCell;
        this.canvas = canvas;
        this.cellSize = cellSize;

        // Der Timer sorgt für das visuelle Flackern (ca. 12,5 FPS bei 80ms)
        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(80)
        };
        timer.Tick += (_, _) => Animate();
    }

    /// <summary>
    /// Startet den Animations-Timer.
    /// </summary>
    public void Start() => timer.Start();

    /// <summary>
    /// Stoppt die Animation und entfernt alle verbleibenden Flammen-Grafiken vom Canvas.
    /// Muss aufgerufen werden, wenn eine Zelle vollständig abgebrannt ist.
    /// </summary>
    public void Stop()
    {
        timer.Stop();
        foreach (var flame in flames)
        {
            canvas.Children.Remove(flame);
        }

        flames.Clear();
    }

    /// <summary>
    /// Führt einen Animationsschritt aus: Löscht alte Flammen und erzeugt neue.
    /// </summary>
    private void Animate()
    {
        // Performance-Hinweis: Das Canvas wird hier bei jedem Tick manipuliert.
        foreach (var flame in flames)
        {
            canvas.Children.Remove(flame);
        }

        flames.Clear();

        SpawnFlames();
    }

    /// <summary>
    /// Erzeugt neue Flammen-Shapes (Ellipsen) an zufälligen Positionen oberhalb der Zelle.
    /// </summary>
    private void SpawnFlames()
    {
        // Mittelpunkt der Zelle auf dem Canvas berechnen
        var centerX = burningCell.X * cellSize + cellSize / 2;
        var centerY = burningCell.Y * cellSize + cellSize / 2;

        var rand = Random.Shared;

        const int flameCount = 1;

        for (var i = 0; i < flameCount; i++)
        {
            // Zufällige Größe für die Flammenform
            var width = rand.NextDouble() * 4 + 4;
            var height = rand.NextDouble() * 8 + 8;

            var flame = new Ellipse
            {
                Width = width,
                Height = height,
                Fill = ColorsData.DefaultFireColor,
                Opacity = 0.85,
                // IsHitTestVisible = false verhindert, dass die Flammen Mausklicks abfangen
                IsHitTestVisible = false
            };

            // 🔥 Positionierung: Leichte Zufälligkeit in X-Richtung, nach oben versetzt in Y-Richtung
            Canvas.SetLeft(flame, centerX - width / 2 + rand.NextDouble() * 6 - 3);
            Canvas.SetTop(flame, centerY - height - rand.NextDouble() * 4);

            // Stellt sicher, dass das Feuer grafisch immer über dem Baum/Gelände liegt
            Panel.SetZIndex(flame, 1000);

            canvas.Children.Add(flame);
            flames.Add(flame);
        }
    }
}
