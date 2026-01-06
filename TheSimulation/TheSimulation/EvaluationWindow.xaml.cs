using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheSimulation;

/// <summary>
/// Interaktionslogik für EvaluationWindow.xaml
/// </summary>
public sealed partial class EvaluationWindow : Window
{
    private readonly EvaluationData data;

    public EvaluationWindow(EvaluationData data)
    {
        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);
        this.data = data;

        EvalTreeDensity.Text = $"Tree Density: {data.ActiveTrees} / {data.MaxTreesPossible} ({data.TreeDensityPercentage:F1}%)";
        EvalRuntime.Text = $"Runtime: {data.Runtime:hh\\:mm\\:ss}";

        Loaded += (_, _) =>
        {
            DrawSimulationChart();
            DrawActiveTreesChart();
        };
    }

    private static void AddLegendItem(Canvas canvas, string text, Brush color, double yOffset, double legendX, double legendY)
    {
        var rect = new Rectangle
        {
            Width = 20,
            Height = 10,
            Fill = color
        };
        Canvas.SetLeft(rect, legendX);
        Canvas.SetTop(rect, legendY + yOffset);
        canvas.Children.Add(rect);

        var label = new TextBlock
        {
            Text = text,
            Foreground = Brushes.Black,
            FontSize = 12
        };
        Canvas.SetLeft(label, legendX + 25);
        Canvas.SetTop(label, legendY + yOffset - 2);
        canvas.Children.Add(label);
    }

    private void DrawSimulationChart()
    {
        if (data.History.Count < 2)
        {
            return;
        }

        ChartCanvas.Children.Clear();

        var width = ChartCanvas.ActualWidth;
        var height = ChartCanvas.ActualHeight;

        if (width == 0)
        {
            width = 800;
        }

        if (height == 0)
        {
            height = 300;
        }

        // Ränder für Achsen
        const double marginLeft = 50;
        const double marginBottom = 30;

        var plotWidth = width - marginLeft - 20;
        var plotHeight = height - marginBottom - 20;

        // Dynamisches Maximum für Y-Achse
        var maxY = Math.Max(1, data.History.Max(h => Math.Max(h.Grown, h.Burned)));

        // Funktion: Koordinaten in Canvas umrechnen
        Point ToCanvasPoint((TimeSpan Time, uint Grown, uint Burned) point, bool forBurned)
        {
            var x = marginLeft + point.Time.TotalSeconds / data.Runtime.TotalSeconds * plotWidth;
            var yValue = forBurned ? point.Burned : point.Grown;
            var y = 10 + plotHeight - (yValue / maxY * plotHeight); // 10 px oben frei
            return new(x, y);
        }

        // --- Linien zeichnen ---
        var grownLine = new Polyline { Stroke = Brushes.Green, StrokeThickness = 2 };
        var burnedLine = new Polyline { Stroke = Brushes.Red, StrokeThickness = 2 };

        foreach (var h in data.History)
        {
            grownLine.Points.Add(ToCanvasPoint(h, false));
            burnedLine.Points.Add(ToCanvasPoint(h, true));
        }

        ChartCanvas.Children.Add(grownLine);
        ChartCanvas.Children.Add(burnedLine);

        // --- Achsen zeichnen ---
        var xAxis = new Line
        {
            X1 = marginLeft,
            Y1 = 10 + plotHeight,
            X2 = marginLeft + plotWidth,
            Y2 = 10 + plotHeight,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        var yAxis = new Line
        {
            X1 = marginLeft,
            Y1 = 10,
            X2 = marginLeft,
            Y2 = 10 + plotHeight,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        ChartCanvas.Children.Add(xAxis);
        ChartCanvas.Children.Add(yAxis);

        // --- Y-Achse Beschriftung ---
        const uint ySteps = 5;
        for (var i = 0; i <= ySteps; i++)
        {
            var yVal = i * maxY / ySteps;
            var y = 10 + plotHeight - (yVal / (double)maxY * plotHeight);

            // Tick
            var tick = new Line
            {
                X1 = marginLeft - 5,
                Y1 = y,
                X2 = marginLeft,
                Y2 = y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            ChartCanvas.Children.Add(tick);

            // Label
            var label = new TextBlock
            {
                Text = ((int)yVal).ToString(),
                FontSize = 12
            };
            Canvas.SetLeft(label, 0);
            Canvas.SetTop(label, y - 10);
            ChartCanvas.Children.Add(label);
        }

        // --- X-Achse Beschriftung (Zeit) ---
        const uint xSteps = 5;
        for (var i = 0; i <= xSteps; i++)
        {
            var tSec = i * data.Runtime.TotalSeconds / xSteps;
            var x = marginLeft + tSec / data.Runtime.TotalSeconds * plotWidth;

            var tick = new Line
            {
                X1 = x,
                Y1 = 10 + plotHeight,
                X2 = x,
                Y2 = 10 + plotHeight + 5,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            ChartCanvas.Children.Add(tick);

            var label = new TextBlock
            {
                Text = TimeSpan.FromSeconds(tSec).ToString(@"mm\:ss"),
                FontSize = 12
            };
            Canvas.SetLeft(label, x - 15);
            Canvas.SetTop(label, 10 + plotHeight + 5);
            ChartCanvas.Children.Add(label);
        }

        AddLegendItem(ChartCanvas, "Total Grown Trees", Brushes.Green, 0, 0, -40);
        AddLegendItem(ChartCanvas, "Total Burned Trees", Brushes.Red, 20, 0, -40);
    }

    private void DrawActiveTreesChart()
    {
        if (data.History.Count < 2)
        {
            return;
        }

        ActiveTreesCanvas.Children.Clear();

        var width = ActiveTreesCanvas.ActualWidth;
        var height = ActiveTreesCanvas.ActualHeight;

        if (width == 0)
        {
            width = 400;
        }

        if (height == 0)
        {
            height = 300;
        }

        const double marginLeft = 50;
        const double marginBottom = 30;
        var plotWidth = width - marginLeft - 20;
        var plotHeight = height - marginBottom - 20;

        // Maximum für aktive Bäume
        var maxActive = Math.Max(1, data.MaxTreesPossible);

        // Polyline für aktive Bäume
        var activeLine = new Polyline
        {
            Stroke = Brushes.DarkGreen,
            StrokeThickness = 2
        };

        foreach (var h in data.History)
        {
            var active = (int)h.Grown - (int)h.Burned;
            var x = marginLeft + h.Time.TotalSeconds / data.Runtime.TotalSeconds * plotWidth;
            var y = 10 + plotHeight - (active / (double)maxActive * plotHeight);
            activeLine.Points.Add(new(x, y));
        }

        ActiveTreesCanvas.Children.Add(activeLine);

        // Achsen
        var xAxis = new Line
        {
            X1 = marginLeft,
            Y1 = 10 + plotHeight,
            X2 = marginLeft + plotWidth,
            Y2 = 10 + plotHeight,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        var yAxis = new Line
        {
            X1 = marginLeft,
            Y1 = 10,
            X2 = marginLeft,
            Y2 = 10 + plotHeight,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        ActiveTreesCanvas.Children.Add(xAxis);
        ActiveTreesCanvas.Children.Add(yAxis);

        // Y-Ticks
        const uint ySteps = 5;
        for (var i = 0; i <= ySteps; i++)
        {
            var yVal = i * maxActive / ySteps;
            var y = 10 + plotHeight - (yVal / maxActive * plotHeight);

            var tick = new Line
            {
                X1 = marginLeft - 5,
                Y1 = y,
                X2 = marginLeft,
                Y2 = y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            ActiveTreesCanvas.Children.Add(tick);

            var label = new TextBlock
            {
                Text = yVal.ToString(),
                FontSize = 12
            };
            Canvas.SetLeft(label, 0);
            Canvas.SetTop(label, y - 10);
            ActiveTreesCanvas.Children.Add(label);
        }

        // X-Ticks
        const uint xSteps = 5;
        for (var i = 0; i <= xSteps; i++)
        {
            var tSec = i * data.Runtime.TotalSeconds / xSteps;
            var x = marginLeft + tSec / data.Runtime.TotalSeconds * plotWidth;

            var tick = new Line
            {
                X1 = x,
                Y1 = 10 + plotHeight,
                X2 = x,
                Y2 = 10 + plotHeight + 5,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            ActiveTreesCanvas.Children.Add(tick);

            var label = new TextBlock
            {
                Text = TimeSpan.FromSeconds(tSec).ToString(@"mm\:ss"),
                FontSize = 12
            };
            Canvas.SetLeft(label, x - 15);
            Canvas.SetTop(label, 10 + plotHeight + 5);
            ActiveTreesCanvas.Children.Add(label);
        }

        AddLegendItem(ActiveTreesCanvas, "Tree count", Brushes.DarkGreen, 0, 0, -20);
    }
}
