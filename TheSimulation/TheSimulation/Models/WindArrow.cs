using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheSimulation;

/// <summary>
/// Represents a directional arrow with a line and a polygon head for the tip.
/// </summary>
public sealed class WindArrow
{
    /// <summary>
    /// The line representing the shaft of the arrow.
    /// </summary>
    public Line Line { get; }

    /// <summary>
    /// The polygon representing the arrowhead.
    /// </summary>
    public Polygon Head { get; }

    private readonly PointCollection points = new(3);
    private const double ArrowHeadLength = 8;
    private const double ArrowHeadWidth = 4;

    /// <summary>
    /// Creates a new Arrow with the specified color.
    /// </summary>
    /// <param name="color">Brush color for both line and arrowhead.</param>
    public WindArrow(Brush color)
    {
        Line = new Line
        {
            Stroke = color,
            StrokeThickness = 3,
            IsHitTestVisible = false
        };
        Head = new Polygon
        {
            Fill = color,
            Points = points,
            IsHitTestVisible = false
        };
    }

    /// <summary>
    /// Draws the arrow from the start position in the given direction with the specified length.
    /// </summary>
    /// <param name="startX">X-coordinate of the arrow base.</param>
    /// <param name="startY">Y-coordinate of the arrow base.</param>
    /// <param name="direction">Normalized vector indicating direction.</param>
    /// <param name="length">Length of the arrow shaft.</param>
    public void Draw(double startX, double startY, Vector direction, double length)
    {
        Line.X1 = startX;
        Line.Y1 = startY;
        Line.X2 = startX + direction.X * length;
        Line.Y2 = startY + direction.Y * length;

        var endX = Line.X2;
        var endY = Line.Y2;

        var normal = new Vector(-direction.Y, direction.X);

        points.Clear();
        points.Add(new(endX, endY));
        points.Add(new(endX - direction.X * ArrowHeadLength + normal.X * ArrowHeadWidth,
                             endY - direction.Y * ArrowHeadLength + normal.Y * ArrowHeadWidth));
        points.Add(new(endX - direction.X * ArrowHeadLength - normal.X * ArrowHeadWidth,
                             endY - direction.Y * ArrowHeadLength - normal.Y * ArrowHeadWidth));
    }
}
