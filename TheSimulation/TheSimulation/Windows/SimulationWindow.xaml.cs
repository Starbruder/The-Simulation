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
    private readonly ParticleGenerator particleGenerator;
    private DispatcherTimer simulationTimer;
    private DateTime simulationStartTime;

    private readonly DispatcherTimer growTimer = new();
    private readonly DispatcherTimer igniteTimer = new();
    private readonly DispatcherTimer fireTimer = new();
    private readonly DispatcherTimer windTimer = new();

    private ForestCellState[,] forestGrid;

    private int cols;
    private int rows;

    private int cachedMaxTreesPossible;
    private float cachedTemperatureEffect;
    private float cachedHumidityEffect;

    private bool IsAnyBurningThenPause = false;

    private uint totalGrownTrees = 0;
    private uint totalBurnedTrees = 0;

    private readonly Dictionary<Cell, Ellipse> treeElements = [];
    private readonly HashSet<Cell> activeTrees = [];

    private readonly List<(TimeSpan Time, uint Grown, uint Burned)> simulationHistory
        = [];

    private TerrainCell[,] terrainGrid;

    public SimulationWindow(SimulationConfig simulationConfig)
    {
        // To get rid of the warning CS8618
        forestGrid = new ForestCellState[0, 0];
        simulationTimer = new();
        terrainGrid = new TerrainCell[0, 0];

        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);

        this.simulationConfig = simulationConfig;
        windHelper = new(simulationConfig);
        windVisualizer = new(ForestCanvas, simulationConfig, windHelper);

        particleGenerator = new ParticleGenerator(ForestCanvas);

        Loaded += async (_, _) => await InitializeSimulationAsync();

        ForestCanvas.MouseLeftButtonDown += (_, e) =>
        {
            var pos = e.GetPosition(ForestCanvas);

            var x = (int)(pos.X / simulationConfig.TreeConfig.Size);
            var y = (int)(pos.Y / simulationConfig.TreeConfig.Size);

			// Löst das Problem, dass außerhalb geklickt wird und das Programm abstürzt.
			// because of out of bounds (Knapp außqerhalb des Baumrasters klicken)
			if (x < 0 || y < 0 || x >= cols || y >= rows)
            {
                return;
            }

            var cell = new Cell(x, y);

            if (forestGrid[x, y] == ForestCellState.Tree)
            {
                forestGrid[x, y] = ForestCellState.Burning;
                UpdateTreeColor(cell, Brushes.Red);
            }
        };
    }

    private async Task InitializeSimulationAsync()
    {
        CacheEnvironmentFactors();
        StartSimulationTimer();
        InitializeGrid();

        if (simulationConfig.PrefillConfig.ShouldPrefillMap)
        {
            await PrefillForest();
        }

        if (simulationConfig.TreeConfig.AllowRegrowForest)
        {
            InitializeGrowTimer();
        }
        InitializeIgniteTimer();
        InitializeFireTimer();

        if (simulationConfig.EnvironmentConfig.WindConfig.RandomDirection ||
            simulationConfig.EnvironmentConfig.WindConfig.RandomStrength)
        {
            InitializeWindTimer();
            return;
        }

        windVisualizer.Draw();
        UpdateWindUI();
    }

    private void CacheEnvironmentFactors()
    {
        cachedTemperatureEffect = CalculateTemperatureEffect();
        cachedHumidityEffect =
            1 - simulationConfig.EnvironmentConfig.AtmosphereConfig.AirHumidityPercentage;
    }

    private void StartSimulationTimer()
    {
        SetAndCalculateStartTime();

        TimerVisualizer.UpdateTimerUI(SimulationTimeText, simulationStartTime);

        simulationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        simulationTimer.Tick += (_, _) =>
        {
            TimerVisualizer.UpdateTimerUI(SimulationTimeText, simulationStartTime);
            RecordSimulationStats();
        };
        simulationTimer.Start();
    }

    private void RecordSimulationStats()
    {
        var elapsed = DateTime.Now - simulationStartTime;
        simulationHistory.Add((elapsed, totalGrownTrees, totalBurnedTrees));
    }

    private void ShowEvaluation_Click(object sender, RoutedEventArgs e)
    {
        var data = new Evaluation
        (
            TotalGrownTrees: totalGrownTrees,
            TotalBurnedTrees: totalBurnedTrees,
            MaxTreesPossible: cachedMaxTreesPossible,
            AirHumidityPercentage:
                simulationConfig.EnvironmentConfig.AtmosphereConfig.AirHumidityPercentage,
            AirTemperatureCelsius:
                simulationConfig.EnvironmentConfig.AtmosphereConfig.AirTemperatureCelsius,
            Runtime: DateTime.Now - simulationStartTime,
            History: new(simulationHistory)
        );

        var evalWindow = new EvaluationWindow(data);
        evalWindow.Show();
    }

    private void SetAndCalculateStartTime()
    {
        // Damit bei erstem Tick 1 Sekunden angezeigt werden.
        var time = DateTime.Now - TimeSpan.FromSeconds(1);
        simulationStartTime = time;
    }

    private void InitializeGrid()
    {
        cols = (int)(ForestCanvas.ActualWidth / simulationConfig.TreeConfig.Size);
        rows = (int)(ForestCanvas.ActualHeight / simulationConfig.TreeConfig.Size);

        forestGrid = new ForestCellState[cols, rows];

        cachedMaxTreesPossible = CalculateMaxTreesPossible();

        if (simulationConfig.TerrainConfig.UseTerrainGeneration)
        {
            terrainGrid = new TerrainCell[cols, rows];
            GenerateTerrain();
        }
    }

    private async Task PrefillForest()
    {
        var maxTrees =
            (int)(cachedMaxTreesPossible * simulationConfig.PrefillConfig.Density);

        // Alle Zellen vorbereiten
        var allCells = new List<Cell>(cols * rows);
        for (var x = 0; x < cols; x++)
        {
            for (var y = 0; y < rows; y++)
            {
                allCells.Add(new(x, y));
            }
        }

        // Shuffle
        for (var i = allCells.Count - 1; i > 0; i--)
        {
            var j = randomHelper.NextInt(0, i + 1);
            (allCells[j], allCells[i]) = (allCells[i], allCells[j]);
        }

        const int batchSize = 200; // optional: Bäume in Paketen laden
        var loaded = 0;

        while (loaded < maxTrees)
        {
            var count = Math.Min(batchSize, maxTrees - loaded);
            var batch = allCells.GetRange(loaded, count);
            loaded += count;

            // UI-Thread zum Hinzufügen der Ellipsen nutzen
            await Dispatcher.InvokeAsync(() =>
            {
                foreach (var cell in batch)
                {
                    AddTree(cell);
                }
            });
        }
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
        igniteTimer.Tick += (_, _) => IgniteRandomTree();
        igniteTimer.Start();
    }

    private void InitializeFireTimer()
    {
        fireTimer.Interval = TimeSpan.FromMilliseconds(SpeedSlider.Value);
        fireTimer.Tick += (_, _) => FireStep();
        fireTimer.Start();
    }

    private void InitializeWindTimer()
    {
        windTimer.Interval = TimeSpan.FromMilliseconds(300); // Aktualisierungsintervall

        windTimer.Tick += (_, _) =>
        {
            windHelper.RandomizeAndUpdateWind(); // Winkel und Strengh randomisieren
            var vector = windHelper.GetWindVector();
            windVisualizer.UpdateWind(vector); // Pfeil aktualisieren

            UpdateWindUI();
        };

        windTimer.Start();
    }

    private void SpeedChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        growTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
        fireTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
        igniteTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue * 750);
        windVisualizer?.Draw();
    }

    protected override void OnClosed(EventArgs e)
    {
        simulationTimer.Stop();
        growTimer.Stop();
        igniteTimer.Stop();
        fireTimer.Stop();
        windTimer.Stop();
        base.OnClosed(e);
    }

    private void GrowStep()
    {
        // Wenn irgendwo Feuer brennt → überspringen
        if (simulationConfig.FireConfig.PauseDuringFire && IsAnyBurningThenPause)
        {
            return;
        }

        // Zielanzahl anhand der Dichte oder Maximalanzahl
        var targetTrees = (int)(cols * rows * simulationConfig.TreeConfig.ForestDensity);

        if (activeTrees.Count >= targetTrees ||
            activeTrees.Count >= simulationConfig.TreeConfig.MaxCount)
        {
            return;
        }

        var cell = randomHelper.NextCell(cols, rows);

        // schon belegt → nichts tun
        if (forestGrid[cell.X, cell.Y] != ForestCellState.Empty)
        {
            return;
        }

        if (!simulationConfig.TerrainConfig.UseTerrainGeneration)
        {
            AddTree(cell);
            return;
        }

        // 🌍 TOPOGRAPHIE-LOGIK
        var terrain = terrainGrid[cell.X, cell.Y];

        // ❌ kein Baum auf Wasser oder Felsen (needs to be uncommented later when impl.)
        //if (terrain.Type != TerrainType.Soil)
        //{
        //    return;
        //}

        // ⛰️ Höhenabhängige Wachstumswahrscheinlichkeit
        // je höher, desto unwahrscheinlicher
        var heightPenalty = terrain.Elevation; // 0.0 – 1.0

        if (randomHelper.NextDouble() < heightPenalty * 0.7)
        {
            return;
        }

        // Baum wächst
        AddTree(cell);
    }

    private double CalculateSlopeEffect(Cell from, Cell to)
    {
        var hFrom = terrainGrid[from.X, from.Y].Elevation;
        var hTo = terrainGrid[to.X, to.Y].Elevation;

        var delta = hTo - hFrom;

        // bergauf → Bonus
        if (delta > 0)
        {
            return 1.0 + delta * 2.0;
        }

        // bergab → Malus
        return 1.0 + delta; // delta ist negativ
    }

    private void AddTree(Cell cell)
    {
        forestGrid[cell.X, cell.Y] = ForestCellState.Tree;

        var color = GetTreeColor(cell);

        var tree = new Ellipse
        {
            Width = simulationConfig.TreeConfig.Size,
            Height = simulationConfig.TreeConfig.Size,
            Fill = color,
            Tag = cell
        };

        Canvas.SetLeft(tree, cell.X * simulationConfig.TreeConfig.Size);
        Canvas.SetTop(tree, cell.Y * simulationConfig.TreeConfig.Size);
        ForestCanvas.Children.Add(tree);

        treeElements[cell] = tree;
        activeTrees.Add(cell);

        totalGrownTrees++;
        UpdateTreeUI();
    }

    private Brush GetTreeColor(Cell cell)
    {
        // Simulate different tree types by using different colors
        var color = randomHelper.NextTreeColor();

        if (simulationConfig.TerrainConfig.UseTerrainGeneration)
        {
            // 🌍 TOPOGRAPHIE-COLOR-LOGIK
            var elevation = terrainGrid[cell.X, cell.Y].Elevation;
            return ColorHelper.AdjustColorByElevation(color, elevation);
        }

        return color;
    }

    private void FireStep()
    {
        windHelper.RandomizeAndUpdateWind();

        // Setzen von toIgnite und toBurnDown
        var toIgnite = new HashSet<Cell>();
        var toBurnDown = new List<Cell>();

        var isFireStepActive = false;

        // Berechnung Wind-Effekt und Verbreitungschancen für alle Zellen im Brandzustand
        var fireSpreadChances = new Dictionary<Cell, double>();

        foreach (var burningCell in treeElements.Keys)
        {
            if (forestGrid[burningCell.X, burningCell.Y] != ForestCellState.Burning)
            {
                continue;
            }

            isFireStepActive = true;

            // Berechne die Chancen für die benachbarten Zellen nur einmal
            foreach (var neighbor in GetNeighbors(burningCell))
            {
                if (forestGrid[neighbor.X, neighbor.Y] != ForestCellState.Tree)
                {
                    continue;
                }

                // Wenn die Chance noch nicht berechnet wurde, berechne sie jetzt
                if (!fireSpreadChances.TryGetValue(neighbor, out var value))
                {
                    value = CalculateFireSpreadChance(burningCell, neighbor);
                    fireSpreadChances[neighbor] = value;
                }

                // Zufällige Zahl ist kleiner als die Berechnete Chance?: ignitiere den Baum
                if (randomHelper.NextDouble() < value)
                {
                    toIgnite.Add(neighbor);
                }
            }

            toBurnDown.Add(burningCell);
        }

        // Zündung von Zellen
        SpreadFire(toIgnite);

        // Brandzerstörung
        BurnDownTrees(toBurnDown);

        IsAnyBurningThenPause = isFireStepActive;
    }

    private float CalculateTemperatureEffect()
    {
        var tempature =
            simulationConfig.EnvironmentConfig.AtmosphereConfig.AirTemperatureCelsius;

        // Referenzbereich für Waldbrandrelevanz
        const int minTemp = 0;
        const int maxTemp = 30;

        var normalized =
            (tempature - minTemp) / (maxTemp - minTemp);

        // Begrenzen
        normalized = Math.Clamp(normalized, 0, 1);

        // leichte Überverstärkung bei extremer Hitze
        const int extremeHeatScaleFaktor = 100;
        if (tempature > maxTemp)
        {
            normalized += (tempature - maxTemp) / extremeHeatScaleFaktor;
        }

        // –20 °C  ⇒  0.0
        // –10 °C  ⇒  0.0
        //  0  °C  ⇒  0.0
        // 15  °C  ⇒  0.5
        // 30  °C  ⇒  1.0
        // 35  °C  ⇒  1.05
        // 40  °C  ⇒  1.10
        return normalized;
    }

    private double CalculateFireSpreadChance(Cell burningCell, Cell neighbor)
    {
        if (simulationConfig.TerrainConfig.UseTerrainGeneration)
        {
            // 🌍 TOPOGRAPHIE-LOGIK
            var terrain = terrainGrid[neighbor.X, neighbor.Y];
            // ❌ kein Feuer auf Wasser oder Felsen
            if (terrain.Type != TerrainType.Soil)
            {
                return 0.0;
            }
        }

        var baseChance =
        simulationConfig.FireConfig.SpreadChancePercent / 100.0;

        var windEffect =
            windHelper.CalculateWindEffect(burningCell, neighbor);

        var chance =
            baseChance *
            windEffect *
            cachedHumidityEffect *
            cachedTemperatureEffect;

        if (simulationConfig.TerrainConfig.UseTerrainGeneration)
        {
            // Höhe → trockener
            var terrain = terrainGrid[neighbor.X, neighbor.Y];
            var elevationEffect = 1.0 + terrain.Elevation * 0.5;

            var slopeEffect = CalculateSlopeEffect(burningCell, neighbor);

            return chance * elevationEffect * slopeEffect;
        }

        return chance;
    }

    private void SpreadFire(HashSet<Cell> toIgnite)
    {
        foreach (var burningCell in toIgnite)
        {
            forestGrid[burningCell.X, burningCell.Y] = ForestCellState.Burning;
            UpdateTreeColor(burningCell, Brushes.Red);

            SpawnFireEffect(burningCell);
        }
    }

    private void SpawnFireEffect(Cell burningCell)
    {
        if (simulationConfig.VisualEffectsConfig.ShowFireParticles
             && randomHelper.NextDouble() < 0.7)
        {
            particleGenerator.AddFireParticle(burningCell, simulationConfig);
            return;
        }

        if (simulationConfig.VisualEffectsConfig.ShowSmokeParticles)
        {
            particleGenerator.AddSmoke(burningCell, simulationConfig);
        }
    }

    private void BurnDownTrees(List<Cell> toBurnDown)
    {
        foreach (var burnedDownCell in toBurnDown)
        {
            BurnDownTree(burnedDownCell);
        }
    }

    private void BurnDownTree(Cell cell)
    {
        // Grid aktualisieren
        forestGrid[cell.X, cell.Y] = ForestCellState.Empty;

        if (treeElements.TryGetValue(cell, out var tree))
        {
            totalBurnedTrees++;

            if (simulationConfig.VisualEffectsConfig.ShowBurnedDownTrees)
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

    private void GenerateTerrain()
    {
        for (var x = 0; x < cols; x++)
        {
            for (var y = 0; y < rows; y++)
            {
                // sanfter Verlauf: Zentrum = hoch, Ränder = niedrig
                var centerX = cols / 2.0;
                var centerY = rows / 2.0;

                var dx = (x - centerX) / centerX; // -1 ... 1
                var dy = (y - centerY) / centerY; // -1 ... 1

                var distance = Math.Sqrt(dx * dx + dy * dy); // 0 = Mitte, 1 = Ecke
                var baseElevation = 1.0 - distance;          // 0 = Rand, 1 = Mitte

                // leichte Zufälligkeit für Hügel
                var noise = randomHelper.NextDouble(-0.1f, 0.1f);

                var elevation = Math.Clamp(baseElevation + noise, 0, 1);

                terrainGrid[x, y] = new TerrainCell
                {
                    Elevation = (float)elevation,
                    Type = TerrainType.Soil
                };
            }
        }
    }

    private void IgniteRandomTree()
    {
        var cell = randomHelper.NextCell(cols, rows);

        if (simulationConfig.VisualEffectsConfig.ShowLightning)
        {
            ShowLightning(cell);
        }

        if (forestGrid[cell.X, cell.Y] == ForestCellState.Tree)
        {
            forestGrid[cell.X, cell.Y] = ForestCellState.Burning;
            UpdateTreeColor(cell, Brushes.Red);
        }
    }

    private async void ShowLightning(Cell cell)
    {
        var lightning = new Ellipse
        {
            Width = simulationConfig.TreeConfig.Size,
            Height = simulationConfig.TreeConfig.Size,
            Fill = Brushes.LightBlue,
            Opacity = 1,
            Tag = cell
        };

        Canvas.SetLeft(lightning, cell.X * simulationConfig.TreeConfig.Size);
        Canvas.SetTop(lightning, cell.Y * simulationConfig.TreeConfig.Size);
        ForestCanvas.Children.Add(lightning);

        await Task.Delay(millisecondsDelay: 150);

        ForestCanvas.Children.Remove(lightning);
    }

    private int CalculateMaxTreesPossible()
        => Math.Max(1, (int)(cols * rows * simulationConfig.TreeConfig.ForestDensity));

    private void UpdateTreeUI()
    {
        TreeDensityText.Text =
            FormatHelper.FormatTreeDensityText(activeTrees.Count, cachedMaxTreesPossible);

        TotalGrownTrees.Text = totalGrownTrees.ToString();
        TotalBurnedTrees.Text = totalBurnedTrees.ToString();
    }

    private void UpdateWindUI()
    {
        // Windstärke im TextBlock anzeigen und den Wert als Prozent
        WindStrengthText.Text = $"{windHelper.CurrentWindStrength * 100:F0}%";
    }
}
