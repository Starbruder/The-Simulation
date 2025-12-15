using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using TheSimulation.Enums;

namespace TheSimulation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private readonly DispatcherTimer growTimer = new();
	private readonly DispatcherTimer igniteTimer = new();
	private readonly DispatcherTimer fireTimer = new();
	private readonly Random random = new();

	private readonly int maxTrees = 1_000_000_000;
	private readonly double treeDensity = 0.6; // 0.0 = leer, 1.0 = voll

	private const int TreeSize = 7;

	private ForestCellState[,] forestGrid;

	private int cols;
	private int rows;

	public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            InitializeGrid();
            InitializeGrowTimer();
            InitializeIgniteTimer();
            InitializeFireTimer();
            InitializeSliders();
        };

		ForestCanvas.MouseLeftButtonDown += (_, e) =>
		{
			var pos = e.GetPosition(ForestCanvas);
			var x = (int)(pos.X / TreeSize);
			var y = (int)(pos.Y / TreeSize);

			if (forestGrid![x, y] == ForestCellState.Tree)
			{
				forestGrid[x, y] = ForestCellState.Burning;
				UpdateTreeColor(x, y, Brushes.Red);
			}
		};
	}

    private void InitializeGrid()
    {
        cols = (int)(ForestCanvas.ActualWidth / TreeSize);
        rows = (int)(ForestCanvas.ActualHeight / TreeSize);

        forestGrid = new ForestCellState[cols, rows];
    }

    private void InitializeGrowTimer()
    {
        growTimer.Interval = TimeSpan.FromMilliseconds(SpeedSlider.Value);
        growTimer.Tick += (s, e) => GrowStep();
        growTimer.Start();
    }

	private void InitializeIgniteTimer()
	{
		fireTimer.Interval = TimeSpan.FromMilliseconds(random.Next());
		fireTimer.Tick += (_, _) => IgniteRandomTree();
		fireTimer.Start();
	}

	private void InitializeFireTimer()
	{
		fireTimer.Interval = TimeSpan.FromMilliseconds(SpeedSlider.Value);
		fireTimer.Tick += (_, _) => FireStep();
		fireTimer.Start();
	}

	private void InitializeSliders()
    {
        SpeedSlider.Minimum = 1;   // schnellster Intervall (ms)
        SpeedSlider.Maximum = 300;  // langsamster Intervall (ms)
        SpeedSlider.Value = 1;     // Startwert
        SpeedSlider.IsDirectionReversed = true; // links = langsam, rechts = schnell
    }

    private void SpeedChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        growTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
		fireTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
	}

    private void GrowStep()
    {
        // Zielanzahl anhand der Dichte oder Maximalanzahl
        var targetTrees = (int)(cols * rows * treeDensity);

        if (ForestCanvas.Children.Count >= targetTrees
            || ForestCanvas.Children.Count >= maxTrees)
        {
            return;
        }

		// zufällige Gitterzelle
		var x = random.Next(cols);
		var y = random.Next(rows);

        // schon belegt → nichts tun
        if (forestGrid[x, y] != ForestCellState.Empty)
        {
            return;
        }

        // Baum wächst
        AddTree(x, y);
    }

	private void AddTree(int x, int y)
	{
		forestGrid[x, y] = ForestCellState.Tree;

		var tree = new Ellipse
		{
			Width = TreeSize,
			Height = TreeSize,
			Fill = Brushes.Green,
			Tag = (x, y) // Grid-Koordinaten speichern
		};

		Canvas.SetLeft(tree, x * TreeSize);
		Canvas.SetTop(tree, y * TreeSize);

		ForestCanvas.Children.Add(tree);
	}

	private void FireStep()
	{
		var toIgnite = new List<(int x, int y)>();
		var toBurnDown = new List<(int x, int y)>();

		for (var x = 0; x < cols; x++)
			for (var y = 0; y < rows; y++)
			{
				if (forestGrid[x, y] != ForestCellState.Burning)
					continue;

				foreach (var (nx, ny) in GetNeighbors(x, y))
				{
					if (forestGrid[nx, ny] == ForestCellState.Tree)
						toIgnite.Add((nx, ny));
				}

				toBurnDown.Add((x, y));
			}

		// 1️⃣ neue Brände
		foreach (var (x, y) in toIgnite.Distinct())
		{
			forestGrid[x, y] = ForestCellState.Burning;
			UpdateTreeColor(x, y, Brushes.Red);
		}

		// 2️⃣ alte Brände abbrennen lassen
		foreach (var (x, y) in toBurnDown)
		{
			BurnDownTree(x, y, true);
		}
	}

	private void BurnDownTree(int x, int y, bool disappear = false)
	{
		forestGrid[x, y] = ForestCellState.Empty;

		if (treeElements[x, y] != null)
		{
			if (disappear)
			{
				ForestCanvas.Children.Remove(treeElements[x, y]);
				treeElements[x, y] = null;
			}
			else
			{
				treeElements[x, y].Fill = Brushes.DarkGray;
			}
		}
	}

	private IEnumerable<(int x, int y)> GetNeighbors(int x, int y)
	{
		for (var dx = -1; dx <= 1; dx++)
			for (var dy = -1; dy <= 1; dy++)
			{
				if (dx == 0 && dy == 0) continue;

				var nx = x + dx;
				var ny = y + dy;

				if (nx >= 0 && ny >= 0 && nx < cols && ny < rows)
					yield return (nx, ny);
			}
	}

	private void UpdateTreeColor(int x, int y, Brush color)
	{
		// 2) Fallback: Suche durch Children (kompatibel zu vorherigem Verhalten)
		foreach (Ellipse tree in ForestCanvas.Children)
		{
			if (tree.Tag is ValueTuple<int, int> tag && tag == (x, y))
			{
				tree.Fill = color;
				return;
			}
		}
	}

	private void IgniteRandomTree()
	{
		const int maxAttempts = 1;

		for (var i = 0; i < maxAttempts; i++)
		{
			var x = random.Next(cols);
			var y = random.Next(rows);

			if (forestGrid[x, y] == ForestCellState.Tree)
			{
				forestGrid[x, y] = ForestCellState.Burning;
				UpdateTreeColor(x, y, Brushes.Red);
				return;
			}
		}
	}
}
