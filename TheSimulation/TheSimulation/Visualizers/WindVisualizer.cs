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

    public void Draw()
    {
        const double margin = 40;
        const double baseLength = 30;

        var centerX = canvas.ActualWidth - margin;
        var centerY = margin;

        var windDir = config.WindDirection;
        var windVector = WindMapper.GetWindVector(windDir);
        if (windVector.Length == 0)
        {
            return;
        }

        windVector.Normalize();

        var length = baseLength * config.WindStrength;

        var endX = centerX + windVector.X * length;
        var endY = centerY + windVector.Y * length;

        arrow ??= new Line
        {
            Stroke = Brushes.LightSkyBlue,
            StrokeThickness = 3,
            IsHitTestVisible = false
        };

        arrow.X1 = centerX;
        arrow.Y1 = centerY;
        arrow.X2 = endX;
        arrow.Y2 = endY;

        var normal = new Vector(-windVector.Y, windVector.X);

        arrowHead ??= new Polygon
        {
            Fill = Brushes.LightSkyBlue,
            IsHitTestVisible = false
        };

        arrowHead.Points =
        [
            new(endX, endY),
            new(endX - windVector.X * 8 + normal.X * 4,
                endY - windVector.Y * 8 + normal.Y * 4),
            new(endX - windVector.X * 8 - normal.X * 4,
                endY - windVector.Y * 8 - normal.Y * 4),
        ];

        if (!canvas.Children.Contains(arrow))
        {
            canvas.Children.Add(arrow);
        }

        if (!canvas.Children.Contains(arrowHead))
        {
            canvas.Children.Add(arrowHead);
        }
    }
}
