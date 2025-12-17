using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TheSimulation;

/// <summary>
/// Interaktionslogik für SimulationWindow.xaml
/// </summary>
public sealed partial class SimulationWindow : Window
{
    private readonly SimulationConfig simulationConfig;

    private readonly RandomHelper randomHelper = new();
    private readonly WindHelper windHelper;
    private readonly WindArrowVisualizer windVisualizer;

    private DispatcherTimer simulationTimer;
    private DateTime simulationStartTime;

    private readonly DispatcherTimer growTimer = new();
    private readonly DispatcherTimer igniteTimer = new();
    private readonly DispatcherTimer fireTimer = new();

    private ForestCellState[,] forestGrid;

    private int cols;
    private int rows;

    private bool IsAnyBurningThenPause = false;

    private uint totalGrownTrees = 0;
    private uint totalBurnedTrees = 0;

    private readonly Dictionary<Cell, Ellipse> treeElements = [];
    private readonly HashSet<Cell> activeTrees = [];

    public SimulationWindow(SimulationConfig simulationConfig)
    {
        // To get rid of the warning CS8618
        forestGrid = new ForestCellState[0, 0];
        simulationTimer = new();

        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);

        this.simulationConfig = simulationConfig;
        windHelper = new(simulationConfig);
        windVisualizer = new(ForestCanvas, simulationConfig);

        Loaded += (_, _) =>
        {
            StartSimulationTimer();
            InitializeGrid();
            InitializeGrowTimer();
            InitializeIgniteTimer();
            InitializeFireTimer();
            InitializeSliders();
            windVisualizer.Draw();
        };

        ForestCanvas.MouseLeftButtonDown += (_, e) =>
        {
            var pos = e.GetPosition(ForestCanvas);

            var x = (int)(pos.X / simulationConfig.TreeSize);
            var y = (int)(pos.Y / simulationConfig.TreeSize);
            var cell = new Cell(x, y);

            if (forestGrid![x, y] == ForestCellState.Tree)
            {
                forestGrid[x, y] = ForestCellState.Burning;
                UpdateTreeColor(cell, Brushes.Red);
            }
        };
    }

    private void StartSimulationTimer()
    {
        SetAndCalculateStartTime();

        TimerVisualizer.UpdateTimerUI(SimulationTimeText, simulationStartTime);

        simulationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        simulationTimer.Tick += (_, _)
            => TimerVisualizer.UpdateTimerUI(SimulationTimeText, simulationStartTime);
        simulationTimer.Start();
    }

    private void SetAndCalculateStartTime()
    {
        // Damit bei erstem Tick 1 Sekunden angezeigt werden.
        var time = DateTime.Now - TimeSpan.FromSeconds(1);
        simulationStartTime = time;
    }

    private void InitializeGrid()
    {
        cols = (int)(ForestCanvas.ActualWidth / simulationConfig.TreeSize);
        rows = (int)(ForestCanvas.ActualHeight / simulationConfig.TreeSize);

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
        igniteTimer.Tick += (_, _) => IgniteRandomTree(simulationConfig.ShowLightning);
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
        windVisualizer?.Draw();
    }

    private void GrowStep()
    {
        // Wenn irgendwo Feuer brennt → überspringen
        if (simulationConfig.PauseDuringFire && IsAnyBurningThenPause)
        {
            return;
        }

        // Zielanzahl anhand der Dichte oder Maximalanzahl
        var targetTrees = (int)(cols * rows * simulationConfig.TreeDensity);

        if (activeTrees.Count >= targetTrees || activeTrees.Count >= simulationConfig.MaxTrees)
        {
            return;
        }

        var cell = randomHelper.GetRandomCell(cols, rows);

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
            Width = simulationConfig.TreeSize,
            Height = simulationConfig.TreeSize,
            Fill = Brushes.Green,
            Tag = cell
        };

        Canvas.SetLeft(tree, cell.X * simulationConfig.TreeSize);
        Canvas.SetTop(tree, cell.Y * simulationConfig.TreeSize);
        ForestCanvas.Children.Add(tree);

        treeElements[cell] = tree;
        activeTrees.Add(cell);

        totalGrownTrees++;
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
                if (forestGrid[neighbor.X, neighbor.Y] != ForestCellState.Tree)
                {
                    continue;
                }

                var chance = CalculateChances(burningCell, neighbor);

                if (randomHelper.NextDouble() < chance)
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
            BurnDownTree(cell, simulationConfig.ReplaceWithBurnedDownTree);
        }

        IsAnyBurningThenPause = isFireStepActive;
    }

    private double CalculateChances(Cell burningCell, Cell neighbor)
    {
        // Zufallschance für Feuerweitergabe berechnen
        var baseChance = simulationConfig.FireSpreadChancePercent / 100f;
        // Zufallschance für Wind-Einfluss berechnen
        var windEffect = windHelper.CalculateWindEffect(burningCell, neighbor);
        var chance = baseChance * windEffect;
        return chance;
    }

    private void BurnDownTree(Cell cell, bool replaceWithBurnedDownTree = false)
    {
        // Grid aktualisieren
        forestGrid[cell.X, cell.Y] = ForestCellState.Empty;

        if (treeElements.TryGetValue(cell, out var tree))
        {
            totalBurnedTrees++;

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
        var cell = randomHelper.GetRandomCell(cols, rows);

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
            Width = simulationConfig.TreeSize,
            Height = simulationConfig.TreeSize,
            Fill = Brushes.LightBlue,
            Opacity = 1,
            Tag = cell
        };

        Canvas.SetLeft(lightning, cell.X * simulationConfig.TreeSize);
        Canvas.SetTop(lightning, cell.Y * simulationConfig.TreeSize);
        ForestCanvas.Children.Add(lightning);

        var removeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(150)
        };
        removeTimer.Tick += (_, _) =>
        {
            ForestCanvas.Children.Remove(lightning);
            removeTimer.Stop();
        };
        removeTimer.Start();
    }

    private int CalculateMaxTreesPossible()
        => Math.Max(1, (int)(cols * rows * simulationConfig.TreeDensity));

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

        TreeDensityText.Text = FormatTreeDensityText(activeTreeCount);

        TotalGrownTrees.Text = totalGrownTrees.ToString();
        TotalBurnedTrees.Text = totalBurnedTrees.ToString();
    }
}
