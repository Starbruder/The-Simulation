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
/// Interaktionslogik für SimulationWindow.xaml
/// </summary>
public sealed partial class SimulationWindow : Window
{
    private DispatcherTimer simulationTimer;
    private DateTime simulationStartTime;

    private readonly DispatcherTimer growTimer = new();
    private readonly DispatcherTimer igniteTimer = new();
    private readonly DispatcherTimer fireTimer = new();

    private readonly Random random = new();

    private const uint maxTrees = 50_000;
    private const float treeDensity = 0.6f; // 0.0 = leer, 1.0 = voll
    private const uint TreeSize = 7;

    private ForestCellState[,] forestGrid;

    private int cols;
    private int rows;

    private const bool replaceWithBurnedDownTree = false;
    private const bool showLightning = false;

    private const bool PauseDuringFire = true;
    private bool IsAnyBurningThenPause = false;

    private readonly Dictionary<Cell, Ellipse> treeElements = [];
    private readonly HashSet<Cell> activeTrees = [];

    public SimulationWindow()
    {
        InitializeComponent();
		InitializeWindowIcon();

        Loaded += (_, _) =>
        {
            StartSimulationTimer();
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

	private void InitializeWindowIcon()
	{
		var iconUri = new Uri("pack://application:,,,/Assets/Images/burning-tree-in-circle.ico");
		Icon = BitmapFrame.Create(iconUri);
	}

	private void StartSimulationTimer()
    {
        simulationStartTime = DateTime.Now;

        simulationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        simulationTimer.Tick += (s, e) =>
        {
            var elapsed = DateTime.Now - simulationStartTime;
            SimulationTimeText.Text = $"Runtime in seconds: {elapsed:hh\\:mm\\:ss}";
        };
        simulationTimer.Start();
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
        growTimer.Tick += (_, _) => GrowStep();
        growTimer.Start();
    }

    private void InitializeIgniteTimer()
    {
        igniteTimer.Interval = TimeSpan.FromMilliseconds(SpeedSlider.Value * 750);
        igniteTimer.Tick += (_, _) => IgniteRandomTree(showLightning);
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
        SpeedSlider.Minimum = 1;
        SpeedSlider.Maximum = 300;
        SpeedSlider.Value = 1;
        SpeedSlider.IsDirectionReversed = true;
    }

    private void SpeedChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        growTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
        fireTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
        igniteTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue * 750);
    }

    private void GrowStep()
    {
        // Wenn irgendwo Feuer brennt → überspringen
        if (PauseDuringFire && IsAnyBurningThenPause)
        {
            return;
        }

        // Zielanzahl anhand der Dichte oder Maximalanzahl
        var targetTrees = (int)(cols * rows * treeDensity);

        if (activeTrees.Count >= targetTrees || activeTrees.Count >= maxTrees)
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

        treeElements[cell] = tree;
        activeTrees.Add(cell);

        UpdateTreeUI();
    }

    private void FireStep()
    {
        var toIgnite = new HashSet<Cell>();
        var toBurnDown = new List<Cell>();

        var isFireStepActive = false;

        foreach (var burningCell in treeElements.Keys)
        {
            if (forestGrid[burningCell.X, burningCell.Y] != ForestCellState.Burning)
            {
                continue;
            }

            isFireStepActive = true;

            foreach (var neighbor in GetNeighbors(burningCell))
            {
                if (forestGrid[neighbor.X, neighbor.Y] == ForestCellState.Tree)
                {
                    toIgnite.Add(neighbor);
                }
            }

            toBurnDown.Add(burningCell);
        }

        // Feuer ausbreiten mit neuen Bränden
        foreach (var cell in toIgnite)
        {
            forestGrid[cell.X, cell.Y] = ForestCellState.Burning;
            UpdateTreeColor(cell, Brushes.Red);
        }

        // alte Brände abbrennen lassen
        foreach (var cell in toBurnDown)
        {
            BurnDownTree(cell, replaceWithBurnedDownTree);
        }

        IsAnyBurningThenPause = isFireStepActive;
    }

    private void BurnDownTree(Cell cell, bool replaceWithBurnedDownTree = false)
    {
        // Grid aktualisieren
        forestGrid[cell.X, cell.Y] = ForestCellState.Empty;

        if (treeElements.TryGetValue(cell, out var tree))
        {
            if (replaceWithBurnedDownTree)
            {
                tree.Fill = Brushes.Gray;
            }
            else
            {
                ForestCanvas.Children.Remove(tree);
                treeElements.Remove(cell);
            }
        }

        activeTrees.Remove(cell);

        UpdateTreeUI();
    }

    private IEnumerable<Cell> GetNeighbors(Cell cell)
    {
        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue; // die Zelle selbst überspringen
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
        if (treeElements.TryGetValue(cell, out var tree))
        {
            tree.Fill = color;
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

    private Cell GetRandomCell() => new(random.Next(cols), random.Next(rows));

    private int CalculateMaxTreesPossible()
        => Math.Max(1, (int)(cols * rows * treeDensity));

    private string FormatTreeDensityText(int activeTreeCount)
    {
        var maxTreesPossible = CalculateMaxTreesPossible();
        var density = activeTreeCount / (float)maxTreesPossible;
        var densityPercent = (int)Math.Round(density * 100);

        return $"{activeTreeCount} / {maxTreesPossible} ({densityPercent}%)";
    }

    private void UpdateTreeUI()
    {
        var activeTreeCount = activeTrees.Count;

        TreeCountText.Text = activeTreeCount.ToString();
        TreeDensityText.Text = FormatTreeDensityText(activeTreeCount);
    }
}
