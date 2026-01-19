using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheSimulation;

public sealed class WindCompasVisualizer
{
    private readonly Canvas canvas;
    private readonly WindConfig config;
    private readonly WindHelper windHelper;

    private readonly Arrow primaryArrow;
    private readonly Arrow oppositeArrow;

    private const double BaseLength = 35;
    private const double ArrowHeadLength = 8;
    private const double ArrowHeadWidth = 4;

    private Vector currentWindVector = new();

    // --- Kompass ---
    private Ellipse compassCircle;
    private readonly TextBlock[] compassLabels = new TextBlock[4];
    private readonly string[] compassNames = ["N", "E", "S", "W"];
    private readonly double[] compassAngles = [270, 0, 90, 180];

    private const double CompassMargin = 20;
    private const double CompassRadius = 50;
    private const double LabelInset = 10;

    public const int OverlayZIndex = 1_000;

    public WindCompasVisualizer(Canvas canvas, WindConfig config, WindHelper windHelper)
    {
        this.canvas = canvas;
        this.config = config;
        this.windHelper = windHelper;

        primaryArrow = new Arrow(Brushes.LightSkyBlue);
        oppositeArrow = new Arrow(Brushes.Red);

        AddToCanvas(primaryArrow.Line);
        AddToCanvas(primaryArrow.Head);
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

    public void UpdateWind(Vector newVector)
    {
        currentWindVector = newVector;
        Draw();
    }

    private Vector GetWindVector()
        => config.RandomDirection ? currentWindVector : WindMapper.GetWindVector(config.Direction);

    public void Draw()
    {
        var (centerX, centerY) = GetCompassCenter();
        var windVector = GetWindVector();

        windVector.Normalize();
        var length = BaseLength * windHelper.CurrentWindStrength;

        primaryArrow.Draw(centerX, centerY, windVector, length, ArrowHeadLength, ArrowHeadWidth);
        oppositeArrow.Draw(centerX, centerY, -windVector, length, ArrowHeadLength, ArrowHeadWidth);
    }

    private (double x, double y) GetCompassCenter()
        => (canvas.ActualWidth - CompassRadius - CompassMargin, CompassRadius + CompassMargin);

    private void CreateCompass()
    {
        var (centerX, centerY) = GetCompassCenter();

        compassCircle ??= new Ellipse();

        compassCircle.Width = compassCircle.Height = CompassRadius * 2;
        compassCircle.Stroke = Brushes.Gray;
        compassCircle.StrokeThickness = 2;
        compassCircle.IsHitTestVisible = false;

        Canvas.SetLeft(compassCircle, centerX - CompassRadius);
        Canvas.SetTop(compassCircle, centerY - CompassRadius);
        AddToCanvas(compassCircle);

        for (var i = 0; i < 4; i++)
        {
            compassLabels[i] = CreateOrUpdateText(compassLabels[i], compassNames[i], centerX, centerY, CompassRadius, compassAngles[i]);
        }
    }

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
        var x = centerX + (radius - LabelInset) * Math.Cos(angleRad);
        var y = centerY + (radius - LabelInset) * Math.Sin(angleRad);

        existing.Measure(new(double.PositiveInfinity, double.PositiveInfinity));
        var size = existing.DesiredSize;

        Canvas.SetLeft(existing, x - size.Width / 2);
        Canvas.SetTop(existing, y - size.Height / 2);

        return existing;
    }

    private void AddToCanvas(UIElement element)
    {
        if (!canvas.Children.Contains(element))
        {
            canvas.Children.Add(element);
        }

        Panel.SetZIndex(element, OverlayZIndex);
    }

    // --- Hilfsklasse für Pfeile ---
    private sealed class Arrow
    {
        public readonly Line Line;
        public readonly Polygon Head;

        public Arrow(Brush color)
        {
            Line = new Line { Stroke = color, StrokeThickness = 3, IsHitTestVisible = false };
            Head = new Polygon { Fill = color, IsHitTestVisible = false };
        }

        public void Draw(double startX, double startY, Vector direction, double length, double headLength, double headWidth)
        {
            var endX = startX + direction.X * length;
            var endY = startY + direction.Y * length;

            Line.X1 = startX;
            Line.Y1 = startY;
            Line.X2 = endX;
            Line.Y2 = endY;

            var normal = new Vector(-direction.Y, direction.X);

            Head.Points.Clear();
            Head.Points.Add(new(endX, endY));
            Head.Points.Add(new(endX - direction.X * headLength + normal.X * headWidth,
                                      endY - direction.Y * headLength + normal.Y * headWidth));
            Head.Points.Add(new(endX - direction.X * headLength - normal.X * headWidth,
                                      endY - direction.Y * headLength - normal.Y * headWidth));
        }
    }
}
