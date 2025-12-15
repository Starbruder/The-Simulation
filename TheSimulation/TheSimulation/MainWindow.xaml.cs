using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using TheSimulation.Enums;
using TheSimulation.Models;

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

    private readonly uint maxTrees = 1_000_000;
    private readonly double treeDensity = 0.6; // 0.0 = leer, 1.0 = voll

    private const int TreeSize = 7;

    private ForestCellState[,] forestGrid;

    private int cols;
    private int rows;

    public MainWindow()
    {
        InitializeComponent();

        var iconUri = new Uri("pack://application:,,,/Assets/Images/burning-tree-in-circle.ico");
        Icon = BitmapFrame.Create(iconUri);

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
			var cell = new Cell((int)(pos.X / TreeSize), (int)(pos.Y / TreeSize));

			if (forestGrid![cell.X, cell.Y] == ForestCellState.Tree)
			{
				forestGrid[cell.X, cell.Y] = ForestCellState.Burning;
				UpdateTreeColor(cell, Brushes.Red);
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
        igniteTimer.Interval = TimeSpan.FromMilliseconds(SpeedSlider.Value * 500); // Faktor anpassen, damit es langsamer als grow/fire ist
        igniteTimer.Tick += (_, _) => IgniteRandomTree();
        igniteTimer.Start();
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
        SpeedSlider.Maximum = 300; // langsamster Intervall (ms)
        SpeedSlider.Value = 1;     // Startwert
        SpeedSlider.IsDirectionReversed = true; // links = langsam, rechts = schnell
    }

    private void SpeedChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        growTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
        fireTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
        igniteTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue * 50);
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

        var cell = GetRandomCell();

        // schon belegt → nichts tun
        if (forestGrid[cell.X, cell.Y] != ForestCellState.Empty)
        {
            return;
        }

        // Baum wächst
        AddTree(cell);
        UpdateTreeCount();
    }

    private void AddTree(Cell cell)
    {
        forestGrid[cell.X, cell.Y] = ForestCellState.Tree;

        var tree = new Ellipse
        {
            Width = TreeSize,
            Height = TreeSize,
            Fill = Brushes.Green,
            Tag = cell
        };

        Canvas.SetLeft(tree, cell.X * TreeSize);
        Canvas.SetTop(tree, cell.Y * TreeSize);
        ForestCanvas.Children.Add(tree);
    }

    private void FireStep()
    {
        var toIgnite = new List<Cell>();
        var toBurnDown = new List<Cell>();

        for (var x = 0; x < cols; x++)
        {
            for (var y = 0; y < rows; y++)
            {
                if (forestGrid[x, y] != ForestCellState.Burning)
                {
                    continue;
                }

                foreach (var neighbor in GetNeighbors(new(x, y)))
                {
                    if (forestGrid[neighbor.X, neighbor.Y] == ForestCellState.Tree)
                    {
                        toIgnite.Add(neighbor);
                    }
                }

                toBurnDown.Add(new(x, y));
            }
        }

        // Feuer ausbreiten mit neuen Bränden
        foreach (var cell in toIgnite.Distinct())
        {
            forestGrid[cell.X, cell.Y] = ForestCellState.Burning;
            UpdateTreeColor(cell, Brushes.Red);
        }

        // 2️⃣ alte Brände abbrennen lassen
        foreach (var cell in toBurnDown)
        {
            BurnDownTree(cell); // replaceWithBurnedDownTree = true → Baum verschwindet und wird durch verbrannten Baum ersetzt
            UpdateTreeCount();
        }
    }

    private void BurnDownTree(Cell cell, bool replaceWithBurnedDownTree = false)
    {
        // Grid aktualisieren
        forestGrid[cell.X, cell.Y] = ForestCellState.Empty;

        if (replaceWithBurnedDownTree)
        {
            UpdateTreeColor(cell, Brushes.Gray);
            return;
        }

        foreach (Ellipse tree in ForestCanvas.Children)
        {
            if (tree.Tag is Cell tag && tag == cell)
            {
                ForestCanvas.Children.Remove(tree);
                break;
            }
        }
    }

    private IEnumerable<Cell> GetNeighbors(Cell cell)
    {
        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                var nx = cell.X + dx;
                var ny = cell.Y + dy;

                if (nx >= 0 && ny >= 0 && nx < cols && ny < rows)
                {
                    yield return new(nx, ny);
                }
            }
        }
    }

    private void UpdateTreeColor(Cell cell, Brush color)
    {
        foreach (Ellipse tree in ForestCanvas.Children)
        {
            if (tree.Tag is Cell tag && tag == cell)
            {
                tree.Fill = color;
                return;
            }
        }
    }

    private void IgniteRandomTree(bool showLightning = false)
    {
        var cell = GetRandomCell();

        if (showLightning)
        {
            ShowLightning(cell);
        }

        if (forestGrid[cell.X, cell.Y] == ForestCellState.Tree)
        {
            forestGrid[cell.X, cell.Y] = ForestCellState.Burning;
            UpdateTreeColor(cell, Brushes.Red);
        }
    }

    private void ShowLightning(Cell cell)
    {
        var lightning = new Ellipse
        {
            Width = TreeSize,
            Height = TreeSize,
            Fill = Brushes.LightBlue,
            Opacity = 1,
            Tag = cell
        };

        Canvas.SetLeft(lightning, cell.X * TreeSize);
        Canvas.SetTop(lightning, cell.Y * TreeSize);
        ForestCanvas.Children.Add(lightning);

        var removeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        removeTimer.Tick += (_, _) =>
        {
            ForestCanvas.Children.Remove(lightning);
            removeTimer.Stop();
        };
        removeTimer.Start();
    }

    private void UpdateTreeCount()
    {
        var treeCount = 0;

        // alle Zellen durchgehen und Bäume zählen
        for (var x = 0; x < cols; x++)
        {
            for (var y = 0; y < rows; y++)
            {
                if (forestGrid[x, y] == ForestCellState.Tree)
                {
                    treeCount++;
                }
            }
        }

        TreeCountText.Text = treeCount.ToString();
    }

    private Cell GetRandomCell() => new(random.Next(cols), random.Next(rows));
}
