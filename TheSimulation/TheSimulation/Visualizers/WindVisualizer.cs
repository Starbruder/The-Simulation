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
public sealed class WindVisualizer(Canvas canvas, SimulationConfig config)
{
    private readonly Canvas canvas = canvas;
    private readonly SimulationConfig config = config;

    private Line? arrow;
    private Polygon? arrowHead;

    public const int OverlayZIndex = 1_000;

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

        arrow ??= CreateLine();
        arrow.X1 = centerX;
        arrow.Y1 = centerY;
        arrow.X2 = endX;
        arrow.Y2 = endY;

        arrowHead ??= CreateArrowHead();
        UpdateArrowHeadPoints(arrowHead, endX, endY, windVector);

        AddOrUpdateCanvasChild(arrow);
        AddOrUpdateCanvasChild(arrowHead);
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

    private static void UpdateArrowHeadPoints(Polygon polygon, double endX, double endY, Vector direction)
    {
        var normal = new Vector(-direction.Y, direction.X);

        polygon.Points =
        [
            new(endX, endY),
            new(endX - direction.X * 8 + normal.X * 4,
                endY - direction.Y * 8 + normal.Y * 4),
            new(endX - direction.X * 8 - normal.X * 4,
                endY - direction.Y * 8 - normal.Y * 4),
        ];
    }

    private void AddOrUpdateCanvasChild(UIElement element)
    {
        if (!canvas.Children.Contains(element))
        {
            canvas.Children.Add(element);
        }

        Canvas.SetZIndex(element, OverlayZIndex);
    }
}
