using Microsoft.VisualBasic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TheSimulation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private DispatcherTimer growTimer = new();
    private Random random = new();

    private int maxTrees = 1_000_000;
    private double treeDensity = 0.6; // 0.0 = leer, 1.0 = voll

	private const int TreeSize = 6;

	private TreeState[,] forestGrid;

	private int cols;
	private int rows;

	public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            InitializeGrid();
            InitializeGrowTimer();
            InitializeSliders();
        };
	}

    private void InitializeGrid()
	{
		cols = (int)(ForestCanvas.ActualWidth / TreeSize);
		rows = (int)(ForestCanvas.ActualHeight / TreeSize);

		forestGrid = new bool[cols, rows];
	}

	private void InitializeGrowTimer()
	{
		growTimer.Interval = TimeSpan.FromMilliseconds(SpeedSlider.Value);
		growTimer.Tick += (s, e) => GrowStep();
		growTimer.Start();
	}

	private void InitializeSliders()
	{
		// Speed-Slider
		SpeedSlider.Minimum = 1;   // schnellster Intervall (ms)
		SpeedSlider.Maximum = 500;  // langsamster Intervall (ms)
		SpeedSlider.Value = 1;     // Startwert
		SpeedSlider.IsDirectionReversed = true; // links = langsam, rechts = schnell
	}

    private void SpeedChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		=> growTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);

    private void GrowStep()
	{
		// Zielanzahl anhand der Dichte oder Maximalanzahl
		int targetTrees = (int)(cols * rows * treeDensity);

		if (ForestCanvas.Children.Count >= targetTrees
            || ForestCanvas.Children.Count >= maxTrees)
		{
			return;
		}

		// zufällige Gitterzelle
		int x = random.Next(cols);
		int y = random.Next(rows);

        // schon belegt → nichts tun
        if (occupied[x, y])
        {
            return;
        }

        // Baum wächst
        occupied[x, y] = true;

		AddTree(
			x * TreeSize,
			y * TreeSize
		);
	}

	private void AddTree(double x, double y)
	{
		var tree = new Ellipse
		{
			Width = 6,
			Height = 6,
			Fill = Brushes.Green
		};

		Canvas.SetLeft(tree, x);
		Canvas.SetTop(tree, y);

		ForestCanvas.Children.Add(tree);
	}
}
