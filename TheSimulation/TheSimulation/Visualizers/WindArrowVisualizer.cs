using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheSimulation;

/// <summary>
/// Provides a visual overlay for displaying wind direction and strength as an arrow on a canvas, using simulation configuration and wind data.
/// </summary>
/// <remarks>
/// The wind arrow is rendered from the center of the canvas,
/// with its orientation and length reflecting the current wind vector.
/// The visualizer updates in response to changes in wind data and is intended for use in
/// simulation or visualization environments where real-time wind feedback is required.
/// The overlay's Z-index is set to ensure it appears above other canvas elements.
/// </remarks>
public sealed class WindArrowVisualizer
{
    private readonly Canvas canvas;
    private readonly SimulationConfig config;
    private readonly WindHelper windHelper;

    private readonly Line arrow;
    private readonly Polygon arrowHead;
    private readonly PointCollection arrowHeadPoints = new(3);

    private readonly static Brush arrowColor = Brushes.LightSkyBlue;

    public const int OverlayZIndex = 1_000;

    private const double Margin = 40;
    private const double BaseLength = 30;
    private const double ArrowHeadLength = 8;
    private const double ArrowHeadWidth = 4;

    private Vector currentWindVector = new();

    /// <summary>
    /// Initializes a new instance of the WindArrowVisualizer class that displays wind direction and speed on the
    /// specified canvas using the provided simulation configuration and wind helper.
    /// </summary>
    /// <param name="canvas">
    /// The Canvas control on which the wind arrow and its head will be rendered.
    /// </param>
    /// <param name="config">
    /// The simulation configuration that determines visual and behavioral parameters for the wind arrow.
    /// </param>
    /// <param name="windHelper">
    /// The WindHelper instance used to obtain wind data for visualization.
    /// </param>
    public WindArrowVisualizer(Canvas canvas, SimulationConfig config, WindHelper windHelper)
    {
        this.canvas = canvas;
        this.config = config;
        this.windHelper = windHelper;

        arrow = CreateLine();
        arrowHead = CreateArrowHead();
        arrowHead.Points = arrowHeadPoints;

        AddToCanvas(arrow);
        AddToCanvas(arrowHead);
    }

    /// <summary>
    /// Updates the current wind vector to the specified value.
    /// </summary>
    /// <param name="newVector">
    /// The new wind vector to apply. Represents the direction and magnitude of the wind.
    /// </param>
    public void UpdateWind(Vector newVector)
    {
        currentWindVector = newVector;

        Draw();
    }

    /// <summary>
    /// Retrieves the current wind vector based on the configured wind direction settings.
    /// </summary>
    /// <returns>
    /// A <see cref="Vector"/> representing the current wind direction and magnitude.
    /// The returned vector reflects either a random direction or a mapped direction,
    /// depending on the configuration.
    /// </returns>
    private Vector GetWindVector()
    {
        if (config.EnvironmentConfig.WindConfig.RandomDirection)
        {
            return currentWindVector;
        }

        return WindMapper.GetWindVector(config.EnvironmentConfig.WindConfig.Direction);
    }

    /// <summary>
    /// Renders a visual representation of the current wind direction and strength on the canvas as an arrow.
    /// </summary>
    /// <remarks>
    /// The arrow is drawn from the center of the canvas, with its length and orientation reflecting the current wind vector.
    /// If there is no wind, nothing is rendered.
    /// </remarks>
    public void Draw()
    {
        var (centerX, centerY) = GetCanvasCenter();
        var windVector = GetWindVector();

        if (windVector.Length == 0)
        {
            return;
        }

        windVector.Normalize();
        var length = BaseLength * windHelper.CurrentWindStrength;

        var endX = centerX + windVector.X * length;
        var endY = centerY + windVector.Y * length;

        DrawArrow(centerX, centerY, endX, endY);
        DrawArrowHead(endX, endY, windVector);
    }

    /// <summary>
    /// Draws an arrow from the specified starting coordinates to the specified ending coordinates.
    /// </summary>
    /// <param name="startX">The X-coordinate of the starting point of the arrow.</param>
    /// <param name="startY">The Y-coordinate of the starting point of the arrow.</param>
    /// <param name="endX">The X-coordinate of the ending point of the arrow.</param>
    /// <param name="endY">The Y-coordinate of the ending point of the arrow.</param>
    private void DrawArrow(double startX, double startY, double endX, double endY)
    {
        arrow.X1 = startX;
        arrow.Y1 = startY;
        arrow.X2 = endX;
        arrow.Y2 = endY;
    }

    /// <summary>
    /// Calculates and updates the points that define the arrowhead at the specified end position and direction.
    /// </summary>
    /// <param name="endX">The X-coordinate of the arrowhead's tip in the drawing space.</param>
    /// <param name="endY">The Y-coordinate of the arrowhead's tip in the drawing space.</param>
    /// <param name="direction">A vector indicating the direction in which the arrowhead should point.</param>
    private void DrawArrowHead(double endX, double endY, Vector direction)
    {
        var points = CalculateArrowHeadPoints(endX, endY, direction);

        arrowHeadPoints.Clear();
        foreach (var p in points)
        {
            arrowHeadPoints.Add(p);
        }
    }

    /// <summary>
    /// Calculates the points that define an arrowhead at a specified end position and direction.
    /// </summary>
    /// <remarks>
    /// The size and width of the arrowhead are determined by the constants used within the method.
    /// The returned points are ordered so that they can be used to draw a filled arrowhead polygon.
    /// </remarks>
    /// <param name="endX">
    /// The X-coordinate of the arrow tip, representing the end point of the arrow.
    /// </param>
    /// <param name="endY">
    /// The Y-coordinate of the arrow tip, representing the end point of the arrow.
    /// </param>
    /// <param name="direction">
    /// A vector indicating the direction in which the arrow is pointing.
    /// The vector's magnitude determines the orientation but not the size of the arrowhead.
    /// </param>
    /// <returns>
    /// An array of three points representing the vertices of the arrowhead polygon.
    /// The first point is the tip of the arrow;
    /// the other two define the base of the arrowhead.
    /// </returns>
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

    /// <summary>
    /// Calculates the coordinates of the center point of the canvas,
    /// adjusted for the specified margin.
    /// </summary>
    /// <returns>
    /// A tuple containing the X and Y coordinates of the canvas center,
    /// with the margin applied.
    /// </returns>
    private (double x, double y) GetCanvasCenter()
        => (canvas.ActualWidth - Margin, Margin);

    /// <summary>
    /// Adds the specified UI element to the canvas if it is not already present and sets its Z-index to ensure correct
    /// overlay ordering.
    /// </summary>
    /// <remarks>
    /// If the element is already a child of the canvas,
    /// its Z-index is updated but it is not added again.
    /// The Z-index is set to the value of OverlayZIndex to control the element's visual stacking order.
    /// </remarks>
    /// <param name="element">
    /// The UI element to add to the canvas. Cannot be null.
    /// </param>
    private void AddToCanvas(UIElement element)
    {
        if (!canvas.Children.Contains(element))
        {
            canvas.Children.Add(element);
        }

        Canvas.SetZIndex(element, OverlayZIndex);
    }

    /// <summary>
    /// Creates a new Line object configured for use as an arrow with predefined appearance and interaction settings.
    /// </summary>
    /// <returns>
    /// A Line instance with the stroke color set to the current arrow color,
    /// a stroke thickness of 3, and hit testing disabled.
    /// </returns>
    private static Line CreateLine() => new()
    {
        Stroke = arrowColor,
        StrokeThickness = 3,
        IsHitTestVisible = false
    };

    /// <summary>
    /// Creates a new Polygon instance configured for use as an arrowhead shape.
    /// </summary>
    /// <returns>
    /// A Polygon object representing an arrowhead,
    /// with its fill color set and hit testing disabled.
    /// </returns>
    private static Polygon CreateArrowHead() => new()
    {
        Fill = arrowColor,
        IsHitTestVisible = false
    };
}
