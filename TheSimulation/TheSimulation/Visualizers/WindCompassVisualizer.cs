using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheSimulation;

/// <summary>
/// Visualizes wind direction and strength on a canvas using arrows.
/// Also includes a small compass with cardinal direction labels (N, E, S, W).
/// </summary>
public sealed class WindCompassVisualizer
{
    private readonly Canvas canvas;
    private readonly WindConfig config;
    private readonly WindHelper windHelper;

    private readonly WindArrow mainArrow;
    private readonly WindArrow oppositeArrow;

    /// <summary>
    /// The Z-index used to ensure the overlay appears above other canvas elements.
    /// </summary>
    public const int OverlayZIndex = 1_000;

    private const double BaseFactor = 20;
    private Vector currentWindVector = new();

    private Ellipse compassCircle;
    private TextBlock[] directionLabels = new TextBlock[4];

    private const double CompassMargin = 20;
    private const double CompassRadius = 50;
    private const double LabelInset = 10;

    /// <summary>
    /// Creates a new WindCompassVisualizer instance on the specified canvas.
    /// </summary>
    /// <param name="canvas">The Canvas on which wind arrows and compass are rendered.</param>
    /// <param name="config">Wind configuration determining direction settings.</param>
    /// <param name="windHelper">Helper providing current wind strength.</param>
    public WindCompassVisualizer(Canvas canvas, WindConfig config, WindHelper windHelper)
    {
        this.canvas = canvas;
        this.config = config;
        this.windHelper = windHelper;

        mainArrow = new WindArrow(Brushes.LightSkyBlue);
        oppositeArrow = new WindArrow(Brushes.Red);

        AddToCanvas(mainArrow.Line);
        AddToCanvas(mainArrow.Head);
        AddToCanvas(oppositeArrow.Line);
        AddToCanvas(oppositeArrow.Head);

        if (canvas.IsLoaded)
        {
            CreateCompass();
        }
        else
        {
            canvas.Loaded += (s, e) => CreateCompass();
        }

        canvas.SizeChanged += (s, e) => CreateCompass();
    }

    /// <summary>
    /// Updates the wind vector and redraws the arrows.
    /// </summary>
    /// <param name="newVector">The new wind vector representing direction and magnitude.</param>
    public void UpdateWind(Vector newVector)
    {
        currentWindVector = newVector;
        Draw();
    }

    /// <summary>
    /// Draws the main and opposite wind arrows on the canvas based on the current wind vector.
    /// </summary>
    public void Draw()
    {
        var (centerX, centerY) = GetCompassCenter();
        var windVector = GetWindVector();

        if (windVector.Length == 0)
        {
            return;
        }

        const int MinArrowLength = 10;
        var length =
            MinArrowLength + BaseFactor * windHelper.CurrentWindStrength;

        windVector.Normalize();

        mainArrow.Draw(centerX, centerY, windVector, length);
        oppositeArrow.Draw(centerX, centerY, -windVector, length);
    }

    /// <summary>
    /// Gets the wind vector based on configuration (random or fixed direction).
    /// </summary>
    private Vector GetWindVector()
    {
        return config.RandomDirection
            ? currentWindVector
            : WindMapper.GetWindVector(config.Direction);
    }

    /// <summary>
    /// Returns the coordinates of the center point for the compass on the canvas.
    /// </summary>
    private (double x, double y) GetCompassCenter()
    {
        var centerX = canvas.ActualWidth - CompassRadius - CompassMargin;
        var centerY = CompassRadius + CompassMargin;
        return (centerX, centerY);
    }

    /// <summary>
    /// Adds a UIElement to the canvas if it is not already present and sets its Z-index.
    /// </summary>
    private void AddToCanvas(UIElement element)
    {
        if (!canvas.Children.Contains(element))
        {
            canvas.Children.Add(element);
        }

        Panel.SetZIndex(element, OverlayZIndex);
    }

    /// <summary>
    /// Creates or updates the compass circle and cardinal direction labels (N, E, S, W).
    /// </summary>
    private void CreateCompass()
    {
        var (centerX, centerY) = GetCompassCenter();

        compassCircle ??= new Ellipse
        {
            Stroke = Brushes.Gray,
            StrokeThickness = 2,
            IsHitTestVisible = false
        };
        compassCircle.Width = compassCircle.Height = CompassRadius * 2;
        Canvas.SetLeft(compassCircle, centerX - CompassRadius);
        Canvas.SetTop(compassCircle, centerY - CompassRadius);
        AddToCanvas(compassCircle);

        var directions = new (string Text, double Angle)[]
        {
            ("N", 270),
            ("E", 0),
            ("S", 90),
            ("W", 180)
        };

        for (var i = 0; i < directions.Length; i++)
        {
            directionLabels[i] = CreateOrUpdateText(
                directionLabels[i],
                directions[i].Text,
                centerX, centerY,
                CompassRadius,
                directions[i].Angle
            );
        }
    }

    /// <summary>
    /// Creates or updates a TextBlock for a compass label.
    /// </summary>
    private TextBlock CreateOrUpdateText(TextBlock existing, string text, double centerX, double centerY, double radius, double angleDegrees)
    {
        if (existing == null)
        {
            existing = new TextBlock
            {
                Text = text,
                Foreground = Brushes.Gray,
                FontWeight = FontWeights.Bold,
                IsHitTestVisible = false,
                RenderTransformOrigin = new(0.5, 0.5)
            };
            AddToCanvas(existing);
        }
        else
        {
            existing.Text = text;
        }

        var angleRad = angleDegrees * Math.PI / 180;
        var adjustedRadius = radius - LabelInset;
        var x = centerX + adjustedRadius * Math.Cos(angleRad);
        var y = centerY + adjustedRadius * Math.Sin(angleRad);

        existing.Measure(new(double.PositiveInfinity, double.PositiveInfinity));
        var size = existing.DesiredSize;

        Canvas.SetLeft(existing, x - size.Width / 2);
        Canvas.SetTop(existing, y - size.Height / 2);

        return existing;
    }
}
