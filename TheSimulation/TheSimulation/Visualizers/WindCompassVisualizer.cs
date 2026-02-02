using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheSimulation;

/// <summary>
/// Visualisiert die Windrichtung und -stärke auf einem Canvas mithilfe von dynamischen Pfeilen.
/// Enthält zusätzlich ein Kompass-Overlay mit Beschriftungen der Haupthimmelsrichtungen (N, O, S, W).
/// </summary>
/// <remarks>
/// Die Klasse berechnet die Positionen in der oberen rechten Ecke des Canvas und reagiert 
/// automatisch auf Größenänderungen des Containers.
/// </remarks>
public sealed class WindCompassVisualizer
{
    private readonly Canvas canvas;
    private readonly WindConfig config;
    private readonly WindHelper windHelper;

    private readonly WindArrow mainArrow;
    private readonly WindArrow oppositeArrow;

    /// <summary>
    /// Der Z-Index für das Wind-Overlay, um sicherzustellen, dass es über der Simulation erscheint.
    /// </summary>
    public const int OverlayZIndex = 1_000;

    private const double BaseFactor = 20;
    private Vector currentWindVector = new();

    private Ellipse compassCircle;
    private readonly Brush compassForeGroundColor = ColorHelper.GetFrozenBrush(Colors.Gray);
    private Ellipse compassBackground;
    private readonly Brush compassBackgroundColor = ColorHelper.GetFrozenBrush(Color.FromArgb(165, 35, 35, 35));
    private readonly TextBlock[] directionLabels = new TextBlock[4];

    private const double CompassMargin = 20;
    private const double CompassRadius = 50;
    private const double LabelInset = 10;

    /// <summary>
    /// Initialisiert eine neue Instanz des <see cref="WindCompassVisualizer"/>.
    /// </summary>
    /// <param name="canvas">Das Ziel-Canvas für die Darstellung.</param>
    /// <param name="config">Die Wind-Konfiguration (Zufall vs. Fixiert).</param>
    /// <param name="windHelper">Hilfsklasse für die aktuelle Windstärke.</param>
    public WindCompassVisualizer(Canvas canvas, WindConfig config, WindHelper windHelper)
    {
        this.canvas = canvas;
        this.config = config;
        this.windHelper = windHelper;

        var frozenMainArrowBrush = ColorHelper.GetFrozenBrush(Colors.LightSkyBlue);
        var frozenOppositeArrowBrush = ColorHelper.GetFrozenBrush(Colors.Red);

        mainArrow = new WindArrow(frozenMainArrowBrush);
        oppositeArrow = new WindArrow(frozenOppositeArrowBrush);

        AddToCanvas(mainArrow.Line);
        AddToCanvas(mainArrow.Head);
        AddToCanvas(oppositeArrow.Line);
        AddToCanvas(oppositeArrow.Head);

        // Sicherstellen, dass der Kompass erst gezeichnet wird, wenn das Canvas geladen ist
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
    /// Aktualisiert den internen Windvektor und löst eine Neuzeichnung der Pfeile aus.
    /// </summary>
    /// <param name="newVector">Der neue Windvektor (Richtung und Magnitude).</param>
    public void Update(Vector newVector)
    {
        currentWindVector = newVector;
        Draw();
    }

    /// <summary>
    /// Berechnet die Pfeillängen basierend auf der Windstärke und zeichnet die Vektorpfeile neu.
    /// </summary>
    public void Draw()
    {
        var (centerX, centerY) = GetCompassCenter();
        var windVector = GetWindVector();

        if (windVector.Length == 0) return;

        const int MinArrowLength = 10;
        var length = MinArrowLength + BaseFactor * windHelper.CurrentWindStrength;

        windVector.Normalize();

        mainArrow.Draw(centerX, centerY, windVector, length);
        oppositeArrow.Draw(centerX, centerY, -windVector, length);
    }

    /// <summary>
    /// Ermittelt den effektiven Windvektor unter Berücksichtigung der Konfiguration.
    /// </summary>
    /// <returns>Ein Vektor, der die aktuelle Windrichtung beschreibt.</returns>
    private Vector GetWindVector()
    {
        return config.RandomDirection
            ? currentWindVector
            : WindMapper.GetWindVector(config.Direction);
    }

    /// <summary>
    /// Berechnet den dynamischen Mittelpunkt des Kompasses relativ zur Canvas-Größe.
    /// </summary>
    private (double x, double y) GetCompassCenter()
    {
        var centerX = canvas.ActualWidth - CompassRadius - CompassMargin;
        var centerY = CompassRadius + CompassMargin;
        return (centerX, centerY);
    }

    /// <summary>
    /// Fügt ein Element zum Canvas hinzu und erzwingt die Platzierung auf der Overlay-Ebene.
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
    /// Erzeugt die grafischen Basiselemente des Kompasses (Kreis und Labels).
    /// </summary>
    private void CreateCompass()
    {
        var (centerX, centerY) = GetCompassCenter();

        compassCircle ??= new Ellipse
        {
            Stroke = compassForeGroundColor,
            StrokeThickness = 2,
            IsHitTestVisible = false
        };
        compassCircle.Width = compassCircle.Height = CompassRadius * 2;
        Canvas.SetLeft(compassCircle, centerX - CompassRadius);
        Canvas.SetTop(compassCircle, centerY - CompassRadius);
        AddToCanvas(compassCircle);

        var directions = new (string Text, double Angle)[]
        {
            ("N", 270), // Norden oben
            ("O", 0),   // Osten rechts (0 Grad in WPF/Trigonometrie)
            ("S", 90),  // Süden unten
            ("W", 180)  // Westen links
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

        AddCompassBackground(centerX, centerY);
    }

    /// <summary>
    /// Erzeugt oder aktualisiert den abgedunkelten Hintergrund des Kompasses für bessere Lesbarkeit.
    /// </summary>
    private void AddCompassBackground(double centerX, double centerY)
    {
        if (compassBackground == null)
        {
            compassBackground = new Ellipse
            {
                Fill = compassBackgroundColor,
                IsHitTestVisible = false
            };
            AddToCanvas(compassBackground);
        }

        compassBackground.Width = compassBackground.Height = CompassRadius * 2;
        Canvas.SetLeft(compassBackground, centerX - CompassRadius);
        Canvas.SetTop(compassBackground, centerY - CompassRadius);

        // Hintergrund soll knapp hinter den Labels und Pfeilen liegen
        Panel.SetZIndex(compassBackground, OverlayZIndex - 2);
    }

    /// <summary>
    /// Hilfsmethode zur trigonometrischen Positionierung der Himmelsrichtungs-Labels.
    /// </summary>
    /// <param name="existing">Ein bereits existierender TextBlock oder null.</param>
    /// <param name="text">Der anzuzeigende Text (N, O, S, W).</param>
    /// <param name="centerX">X-Koordinate des Zentrums.</param>
    /// <param name="centerY">Y-Koordinate des Zentrums.</param>
    /// <param name="radius">Radius des Kompasses.</param>
    /// <param name="angleDegrees">Winkel der Position in Grad.</param>
    /// <returns>Ein konfigurierter TextBlock.</returns>
    private TextBlock CreateOrUpdateText(TextBlock existing, string text, double centerX, double centerY, double radius, double angleDegrees)
    {
        if (existing == null)
        {
            existing = new TextBlock
            {
                Text = text,
                Foreground = compassForeGroundColor,
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

        // Trigonometrische Berechnung: Polar zu Kartesisch
        var angleRad = angleDegrees * Math.PI / 180;
        var adjustedRadius = radius - LabelInset;
        var x = centerX + adjustedRadius * Math.Cos(angleRad);
        var y = centerY + adjustedRadius * Math.Sin(angleRad);

        existing.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var size = existing.DesiredSize;

        // Zentrierung des Textes auf dem berechneten Punkt
        Canvas.SetLeft(existing, x - size.Width / 2);
        Canvas.SetTop(existing, y - size.Height / 2);

        return existing;
    }
}
