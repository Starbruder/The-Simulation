using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheSimulation.Visualizers;

/// <summary>
/// WindHelper       → Logik
/// WindVisualizer   → Darstellung
/// SimulationWindow → Orchestriert
/// </summary>
public sealed class WindArrowVisualizer
{
    private readonly Canvas canvas;
    private readonly SimulationConfig config;

    private readonly Line arrow;
    private readonly Polygon arrowHead;
    private readonly PointCollection arrowHeadPoints = new(3);

    public const int OverlayZIndex = 1_000;

    public WindArrowVisualizer(Canvas canvas, SimulationConfig config)
    {
        this.canvas = canvas;
        this.config = config;

        // Shapes einmal erstellen
        arrow = CreateLine();
        arrowHead = CreateArrowHead();
        arrowHead.Points = arrowHeadPoints;

        canvas.Children.Add(arrow);
        canvas.Children.Add(arrowHead);

        Canvas.SetZIndex(arrow, OverlayZIndex);
        Canvas.SetZIndex(arrowHead, OverlayZIndex);
    }

    public void Draw()
    {
        const double margin = 40;
        const double baseLength = 30;

        var centerX = canvas.ActualWidth - margin;
        var centerY = margin;

        var windVector = WindMapper.GetWindVector(config.WindDirection);
        if (windVector.Length == 0)
        {
            return;
        }

        windVector.Normalize();
        var length = baseLength * config.WindStrength;

        var endX = centerX + windVector.X * length;
        var endY = centerY + windVector.Y * length;

        // Line aktualisieren
        arrow.X1 = centerX;
        arrow.Y1 = centerY;
        arrow.X2 = endX;
        arrow.Y2 = endY;

        // Pfeilspitze aktualisieren
        UpdateArrowHeadPoints(endX, endY, windVector);
    }

    private static Line CreateLine() => new()
    {
        Stroke = Brushes.LightSkyBlue,
        StrokeThickness = 3,
        IsHitTestVisible = false
    };

    private static Polygon CreateArrowHead() => new()
    {
        Fill = Brushes.LightSkyBlue,
        IsHitTestVisible = false
    };

    private void UpdateArrowHeadPoints(double endX, double endY, Vector direction)
    {
        var normal = new Vector(-direction.Y, direction.X);

        if (arrowHeadPoints.Count != 3)
        {
            arrowHeadPoints.Clear();
            arrowHeadPoints.Add(new Point(endX, endY));
            arrowHeadPoints.Add(new Point(endX - direction.X * 8 + normal.X * 4,
                                          endY - direction.Y * 8 + normal.Y * 4));
            arrowHeadPoints.Add(new Point(endX - direction.X * 8 - normal.X * 4,
                                          endY - direction.Y * 8 - normal.Y * 4));
        }
        else
        {
            arrowHeadPoints[0] = new Point(endX, endY);
            arrowHeadPoints[1] = new Point(endX - direction.X * 8 + normal.X * 4,
                                           endY - direction.Y * 8 + normal.Y * 4);
            arrowHeadPoints[2] = new Point(endX - direction.X * 8 - normal.X * 4,
                                           endY - direction.Y * 8 - normal.Y * 4);
        }
    }
}
