using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheSimulation;

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

    private readonly static Brush arrowColor = Brushes.LightSkyBlue;

    public const int OverlayZIndex = 1_000;

    private const double Margin = 40;
    private const double BaseLength = 30;
    private const double ArrowHeadLength = 8;
    private const double ArrowHeadWidth = 4;

	private Vector currentWindVector = new(1, 0); // default nach rechts

	public WindArrowVisualizer(Canvas canvas, SimulationConfig config)
    {
        this.canvas = canvas;
        this.config = config;

        arrow = CreateLine();
        arrowHead = CreateArrowHead();
        arrowHead.Points = arrowHeadPoints;

        AddToCanvas(arrow);
        AddToCanvas(arrowHead);
    }

	public void UpdateWind(Vector newVector)
	{
		currentWindVector = newVector;
		Draw();
	}

	public void Draw()
	{
		var (centerX, centerY) = GetCanvasCenter();
		var windVector = currentWindVector;

		if (windVector.Length == 0)
        {
            return;
        }

        windVector.Normalize();
		var length = BaseLength * config.WindConfig.Strength;

		var endX = centerX + windVector.X * length;
		var endY = centerY + windVector.Y * length;

		DrawArrow(centerX, centerY, endX, endY);
		DrawArrowHead(endX, endY, windVector);
	}

	private void DrawArrow(double startX, double startY, double endX, double endY)
    {
        arrow.X1 = startX;
        arrow.Y1 = startY;
        arrow.X2 = endX;
        arrow.Y2 = endY;
    }

    private void DrawArrowHead(double endX, double endY, Vector direction)
    {
        var points = CalculateArrowHeadPoints(endX, endY, direction);

        arrowHeadPoints.Clear();
        foreach (var p in points)
        {
            arrowHeadPoints.Add(p);
        }
    }

    private static Point[] CalculateArrowHeadPoints(double endX, double endY, Vector direction)
    {
        var normal = new Vector(-direction.Y, direction.X);

        return
        [
            new(endX, endY),
            new(endX - direction.X * ArrowHeadLength + normal.X * ArrowHeadWidth,
                      endY - direction.Y * ArrowHeadLength + normal.Y * ArrowHeadWidth),
            new(endX - direction.X * ArrowHeadLength - normal.X * ArrowHeadWidth,
                      endY - direction.Y * ArrowHeadLength - normal.Y * ArrowHeadWidth)
        ];
    }

    private (double x, double y) GetCanvasCenter()
        => (canvas.ActualWidth - Margin, Margin);

    private void AddToCanvas(UIElement element)
    {
        if (!canvas.Children.Contains(element))
        {
            canvas.Children.Add(element);
        }

        Canvas.SetZIndex(element, OverlayZIndex);
    }

    private static Line CreateLine() => new()
    {
        Stroke = arrowColor,
        StrokeThickness = 3,
        IsHitTestVisible = false
    };

    private static Polygon CreateArrowHead() => new()
    {
        Fill = arrowColor,
        IsHitTestVisible = false
    };
}
