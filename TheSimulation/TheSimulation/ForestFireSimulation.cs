using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TheSimulation;

/// <summary>
/// Repräsentiert den gesamten Simulationsprozess einer Waldbrand-Simulation.
/// </summary>
public sealed class ForestFireSimulation
{
    private Canvas ForestCanvas { get; }

    private readonly SimulationConfig simulationConfig;

    public Action<string> UpdateSimulationTimeText;
    public Action<string> UpdateTreeDensityText;
    public Action<string> UpdateWindStrengthText;
    public Action<string> UpdateTotalGrownTreesText;
    public Action<string> UpdateTotalBurnedTreesText;

    private readonly RandomHelper randomHelper = new();
    private readonly WindHelper windHelper;
    private readonly WindCompassVisualizer windVisualizer;
    private readonly ParticleGenerator particleGenerator;
    private readonly DispatcherTimer simulationTimer = new();
    private TimeSpan accumulatedSimulationTime = TimeSpan.Zero;

    private readonly DispatcherTimer treeGrowthTimer = new();
    private readonly DispatcherTimer igniteTimer = new();
    private readonly DispatcherTimer fireTimer = new();
    private readonly DispatcherTimer windUpdateTimer = new();

    private ForestGrid grid = new(0, 0);

    private int cachedMaxTreesPossible;
    private float cachedTemperatureEffect;
    private float cachedHumidityEffect;

    public bool isPaused = false;
    private bool isFireActiveThenPause = false;

    private uint totalGrownTrees = 0;
    private uint totalBurnedTrees = 0;

    const uint windChangeIntervalMs = 300 + (int)SimulationSpeed.Normal;
    private SimulationSpeed simulationSpeed = SimulationSpeed.Normal;

    private readonly Dictionary<Cell, Shape> treeElements = [];
    private readonly HashSet<Cell> activeTrees = [];
    private readonly HashSet<Cell> growableCells = [];
    private readonly HashSet<Cell> burningTrees = [];

    private readonly Dictionary<Cell, FireAnimation> fireAnimations = [];

    private readonly List<SimulationSnapshot> simulationHistory = [];

    private readonly List<FireEvent> fireEventsHistory = [];

    private TerrainCell[,] terrainGrid;

    private readonly Rectangle screenFlash = new();

    public ForestFireSimulation(SimulationConfig simulationConfig, Canvas ForestCanvas)
    {
        // To get rid of the warning CS8618
        terrainGrid = new TerrainCell[0, 0];

        this.ForestCanvas = ForestCanvas;

        this.simulationConfig = simulationConfig;
        windHelper = new(simulationConfig.EnvironmentConfig.WindConfig);
        windVisualizer =
            new(ForestCanvas, simulationConfig.EnvironmentConfig.WindConfig, windHelper);

        particleGenerator = new ParticleGenerator(ForestCanvas);

        ForestCanvas.Loaded += async (_, _) => await InitializeSimulationAsync();

        ForestCanvas.MouseLeftButtonDown += (_, e) =>
        {
            MouseBurnClick(simulationConfig, e);
        };

        ForestCanvas.MouseRightButtonDown += (_, e) =>
        {
            MouseDestroyClick(e);
        };

        StartOrResumeSimulation();
    }

    private void MouseDestroyClick(MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(ForestCanvas);

        var x = (int)(pos.X / simulationConfig.TreeConfig.Size);
        var y = (int)(pos.Y / simulationConfig.TreeConfig.Size);

        // Löst das Problem, dass außerhalb geklickt wird und das Programm abstürzt.
        // of out of bounds (Knapp außerhalb des Baumrasters klicken)
        var cell = new Cell(x, y);
        if (!grid.IsInside(cell))
        {
            return;
        }

        DestroyCell(cell);
    }

    private void DestroyCell(Cell cell)
    {
        if (!grid.IsTree(cell))
        {
            return;
        }

        // 🔥 Falls der Baum brennt → Feuer & Effekte stoppen
        if (burningTrees.Remove(cell))
        {
            if (fireAnimations.TryGetValue(cell, out var fire))
            {
                fire.Stop();
                fireAnimations.Remove(cell);
            }
        }

        grid.Clear(cell);

        // 🖼️ UI-Element entfernen
        if (treeElements.TryGetValue(cell, out var tree))
        {
            ForestCanvas.Children.Remove(tree);
            treeElements.Remove(cell);
        }

        activeTrees.Remove(cell);
        growableCells.Add(cell);

        UpdateTreeUI();
    }

    public void StartOrResumeSimulation()
    {
        isPaused = false;

        if (simulationConfig.TreeConfig.AllowRegrowForest)
        {
            treeGrowthTimer.Start();
        }

        igniteTimer.Start();
        fireTimer.Start();
        simulationTimer.Start();

        if (simulationConfig.EnvironmentConfig.WindConfig.RandomDirection ||
            simulationConfig.EnvironmentConfig.WindConfig.RandomStrength)
        {
            windUpdateTimer.Start();
            return;
        }

        windVisualizer.Draw();
        UpdateWindUI();
    }

    private void MouseBurnClick
        (SimulationConfig simulationConfig, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(ForestCanvas);

        var x = (int)(pos.X / simulationConfig.TreeConfig.Size);
        var y = (int)(pos.Y / simulationConfig.TreeConfig.Size);

        // Löst das Problem, dass außerhalb geklickt wird und das Programm abstürzt.
        // of out of bounds (Knapp außerhalb des Baumrasters klicken)
        var cell = new Cell(x, y);
        if (!grid.IsInside(cell))
        {
            return;
        }

        IgniteTree(cell);
        fireEventsHistory.Add(new FireEvent(
            FireEventType.ManualIgnition,
            accumulatedSimulationTime
        ));
    }

    private void IgniteTree(Cell cell)
    {
        if (!grid.IsTree(cell))
        {
            return;
        }

        grid.SetBurning(cell);
        burningTrees.Add(cell);
        UpdateTreeColor(cell, Brushes.Red);

        SpawnFireEffect(cell);
    }

    private async Task InitializeSimulationAsync()
    {
        CacheEnvironmentFactors();
        InitializeSimulationTimer();
        InitializeGrid();

        if (simulationConfig.VisualEffectsConfig.ShowBoltScreenFlash)
        {
            InitializeScreenFlash();
        }

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

        const SimulationSpeed startSimulationSpeed = SimulationSpeed.Normal;
        SetSimulationSpeed(startSimulationSpeed);

        if (simulationConfig.EnvironmentConfig.WindConfig.RandomDirection ||
            simulationConfig.EnvironmentConfig.WindConfig.RandomStrength)
        {
            InitializeWindTimer();
            return;
        }

        windVisualizer.Draw();
        UpdateWindUI();
    }

    private void InitializeScreenFlash()
    {
        screenFlash.Width = ForestCanvas.ActualWidth;
        screenFlash.Height = ForestCanvas.ActualHeight;
        screenFlash.Fill = Brushes.White;

        Panel.SetZIndex(screenFlash, int.MaxValue);

        ForestCanvas.Children.Add(screenFlash);
    }

    private void CacheEnvironmentFactors()
    {
        cachedTemperatureEffect =
            TemperatureHelper.CalculateTemperatureEffect(simulationConfig.EnvironmentConfig.AtmosphereConfig);
        cachedHumidityEffect =
            1 - simulationConfig.EnvironmentConfig.AtmosphereConfig.AirHumidityPercentage;
    }

    private void InitializeSimulationTimer()
    {
        var hours = (int)accumulatedSimulationTime.TotalHours;
        UpdateSimulationTimeText?.Invoke(
            $"Runtime: {hours:D2}:{accumulatedSimulationTime.Minutes:D2}:{accumulatedSimulationTime.Seconds:D2}"
        );

        simulationTimer.Interval = TimeSpan.FromSeconds(1);

        simulationTimer.Tick += (_, _) =>
        {
            const int maxSimulationHours = 99;
            if (CalculateSimulationTime() >= new TimeSpan(maxSimulationHours, 0, 0)
            || simulationConfig.PrefillConfig.ShouldPrefillMap && LowDensityMinimumReached())
            {
                StopOrPauseSimulation();
                OpenEvalualtionWindow();
                return;
            }

            accumulatedSimulationTime =
                accumulatedSimulationTime.Add(simulationTimer.Interval);

            var hours = (int)accumulatedSimulationTime.TotalHours;
            UpdateSimulationTimeText?.Invoke(
                $"Runtime: {hours:D2}:{accumulatedSimulationTime.Minutes:D2}:{accumulatedSimulationTime.Seconds:D2}"
            );
            RecordSimulationStats();
        };
    }

    private bool LowDensityMinimumReached()
    {
        const uint lowDensityMinimumPercent = 3;
        return CalculateCurrentTreeDensityPercent() <= lowDensityMinimumPercent;
    }

    private double CalculateCurrentTreeDensityPercent()
    {
        var density = activeTrees.Count / (double)cachedMaxTreesPossible;
        var densityPercent = Math.Round(density * 100);
        return densityPercent;
    }

    public void StopOrPauseSimulation()
    {
        isPaused = true;

        simulationTimer.Stop();
        treeGrowthTimer.Stop();
        igniteTimer.Stop();
        fireTimer.Stop();
        windUpdateTimer.Stop();
    }

    private void RecordSimulationStats()
    {
        var elapsedTime = CalculateSimulationTime();
        var currentWindSpeed = windHelper.CurrentWindStrength;
        var historySnapshot = new SimulationSnapshot
        (
            elapsedTime,
            totalGrownTrees,
            totalBurnedTrees,
            currentWindSpeed
        );
        simulationHistory.Add(historySnapshot);
    }

    private TimeSpan CalculateSimulationTime() => accumulatedSimulationTime;

    public void OpenEvalualtionWindow()
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
            Runtime: accumulatedSimulationTime,
            History: new(simulationHistory),
            FireEvents: new(fireEventsHistory)
        );

        var evalWindow = new EvaluationWindow(data);
        evalWindow.Show();
    }

    private void InitializeGrid()
    {
        var cols =
            (int)(ForestCanvas.ActualWidth / simulationConfig.TreeConfig.Size);
        var rows =
            (int)(ForestCanvas.ActualHeight / simulationConfig.TreeConfig.Size);

        grid = new(cols, rows);

        cachedMaxTreesPossible = CalculateMaxTreesPossible();

        if (simulationConfig.TerrainConfig.UseTerrainGeneration)
        {
            terrainGrid = new TerrainCell[cols, rows];
            GenerateTerrain();
        }

        InitializeGrowableCells();
    }

    private void InitializeGrowableCells()
    {
        growableCells.Clear();

        for (var x = 0; x < grid.Cols; x++)
        {
            for (var y = 0; y < grid.Rows; y++)
            {
                if (!simulationConfig.TerrainConfig.UseTerrainGeneration)
                {
                    growableCells.Add(new(x, y));
                    continue;
                }

                var terrain = terrainGrid[x, y];

                if (terrain.Type == TerrainType.Soil)
                {
                    growableCells.Add(new(x, y));
                }
            }
        }
    }

    private async Task PrefillForest()
    {
        var maxTrees =
            (int)(cachedMaxTreesPossible * simulationConfig.PrefillConfig.Density);

        var allCells = GenerateCells();

        ShuffleCells(allCells);

        const int treesBatchSize = 200;
        await LoadTreesInBatches(maxTrees, allCells, treesBatchSize);
    }

    private async Task LoadTreesInBatches(int maxTrees, List<Cell> allCells, int treesBatchSize)
    {
        var loaded = 0;

        while (loaded < maxTrees)
        {
            var count = Math.Min(treesBatchSize, maxTrees - loaded);
            var batch = allCells.GetRange(loaded, count);
            loaded += count;

            // UI-Thread zum Hinzufügen der TreeShapes nutzen
            await ForestCanvas.Dispatcher.InvokeAsync(() =>
            {
                foreach (var cell in batch)
                {
                    AddTreeWithoutUIUpdate(cell);
                }
            });
        }

        UpdateTreeUI();
    }

    private void ShuffleCells(List<Cell> allCells)
    {
        for (var i = allCells.Count - 1; i > 0; i--)
        {
            var j = randomHelper.NextInt(0, i + 1);
            (allCells[j], allCells[i]) = (allCells[i], allCells[j]);
        }
    }

    private List<Cell> GenerateCells()
    {
        var allCells = new List<Cell>(grid.Cols * grid.Rows);

        for (var x = 0; x < grid.Cols; x++)
        {
            for (var y = 0; y < grid.Rows; y++)
            {
                allCells.Add(new(x, y));
            }
        }

        return allCells;
    }

    private void InitializeGrowTimer()
    {
        treeGrowthTimer.Tick += (_, _) => GrowStep();
    }

    private void InitializeIgniteTimer()
    {
        igniteTimer.Tick += async (_, _) =>
        {
            try
            {
                await IgniteRandomCell();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                StopOrPauseSimulation();
            }
        };
    }

    private void InitializeFireTimer()
    {
        fireTimer.Tick += (_, _) => FireStep();
    }

    private void InitializeWindTimer()
    {
        windUpdateTimer.Interval = TimeSpan.FromMilliseconds(windChangeIntervalMs);

        windUpdateTimer.Tick += (_, _) =>
        {
            windHelper.RandomizeAndUpdateWind(); // Winkel und Strengh randomisieren
            var vector = windHelper.GetWindVector();
            windVisualizer.UpdateWind(vector); // Pfeil aktualisieren

            UpdateWindUI();
        };
    }

    public void SetSimulationSpeed(SimulationSpeed simulationSpeed)
    {
        this.simulationSpeed = simulationSpeed;
        var baseIntervalMs = (int)simulationSpeed;

        treeGrowthTimer.Interval = TimeSpan.FromMilliseconds(baseIntervalMs);
        fireTimer.Interval = TimeSpan.FromMilliseconds(baseIntervalMs);
        igniteTimer.Interval = TimeSpan.FromMilliseconds(baseIntervalMs * 750);
        windUpdateTimer.Interval = TimeSpan.FromMilliseconds(baseIntervalMs + windChangeIntervalMs);

        windVisualizer?.Draw();
    }

    private void GrowStep()
    {
        // Wenn irgendwo Feuer brennt → überspringen
        if (simulationConfig.FireConfig.PauseDuringFire && isFireActiveThenPause)
        {
            return;
        }

        // Zielanzahl anhand der Dichte oder Maximalanzahl
        var targetTrees = (int)(grid.Cols * grid.Rows * simulationConfig.TreeConfig.ForestDensity);

        if (activeTrees.Count >= targetTrees ||
            activeTrees.Count >= simulationConfig.TreeConfig.MaxCount)
        {
            return;
        }

        var cell = randomHelper.NextCell(growableCells);

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
        const double HeightPenaltyFactor = 0.7;
        if (randomHelper.NextDouble() < heightPenalty * HeightPenaltyFactor)
        {
            return;
        }

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
            return 1 + delta * 2;
        }

        // bergab → Malus
        return 1 + delta; // delta ist negativ
    }

    private void AddTree(Cell cell)
    {
        growableCells.Remove(cell);
        AddTreeWithoutUIUpdate(cell);
        UpdateTreeUI();
    }

    private void AddTreeWithoutUIUpdate(Cell cell)
    {
        grid.SetTree(cell);

        var color = GetTreeColor(cell);
        var tree = CreateCellShape(cell, color);

        Canvas.SetLeft(tree, cell.X * simulationConfig.TreeConfig.Size);
        Canvas.SetTop(tree, cell.Y * simulationConfig.TreeConfig.Size);
        ForestCanvas.Children.Add(tree);

        treeElements[cell] = tree;
        activeTrees.Add(cell);

        totalGrownTrees++;
    }

    private Shape CreateCellShape(Cell cell, Brush color)
    {
        var size = simulationConfig.TreeConfig.Size;

        return simulationConfig.VisualEffectsConfig.TreeShape switch
        {
            Ellipse => new Ellipse
            {
                Width = size,
                Height = size,
                Fill = color,
                Tag = cell
            },
            Rectangle => new Rectangle
            {
                Width = size,
                Height = size,
                Fill = color,
                Tag = cell
            },
            _ => throw new NotSupportedException("Unsupported tree shape")
        };
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
        if (burningTrees.Count == 0)
        {
            isFireActiveThenPause = false;
            return;
        }

        windHelper.RandomizeAndUpdateWind();

        var toIgnite = new HashSet<Cell>();
        var toBurnDown = new List<Cell>();

        foreach (var burningCell in burningTrees)
        {
            foreach (var neighbor in grid.GetNeighbors(burningCell))
            {
                if (randomHelper.NextDouble() <
                    simulationConfig.FireConfig.SpreadChancePercent / 100)
                {
                    // Durch Funkenflug auch weiter entfernte Bäume anzünden
                    TryIgniteNearbyCell(burningCell, toIgnite);
                }

                if (!grid.IsTree(neighbor))
                {
                    continue;
                }

                var chance = CalculateFireSpreadChance(burningCell, neighbor);
                if (randomHelper.NextDouble() < chance)
                {
                    toIgnite.Add(neighbor);
                }
            }

            toBurnDown.Add(burningCell);
        }

        SpreadFire(toIgnite);
        BurnDownTrees(toBurnDown);

        isFireActiveThenPause = burningTrees.Count > 0;
    }

    /// <summary>
    /// Versucht, durch Hitze oder Funkenflug einen zusätzlichen Baum
    /// im Umkreis von 2–3 Zellen zu entzünden.
    /// Die Wahrscheinlichkeit nimmt mit der Distanz ab und wird stark
    /// von Windrichtung und -stärke beeinflusst.
    /// </summary>
    /// <param name="source">Die aktuell brennende Zelle</param>
    /// <param name="toIgnite">Sammlung neu zu entzündender Zellen im aktuellen FireStep</param>
    private void TryIgniteNearbyCell(Cell source, HashSet<Cell> toIgnite)
    {
        const int MinSpotFireRadius = 2;
        const int MaxSpotFireRadius = 4;

        // Zufällige Verschiebung der Zielzelle in X- und Y-Richtung
        var offsetX = randomHelper.NextInt(-MaxSpotFireRadius, MaxSpotFireRadius + 1);
        var offsetY = randomHelper.NextInt(-MaxSpotFireRadius, MaxSpotFireRadius + 1);

        // Abstand zur Zielzelle berechnen (Hypotenuse)
        var distanceToTarget = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);

        if (distanceToTarget < MinSpotFireRadius || distanceToTarget > MaxSpotFireRadius)
        {
            return;
        }

        var target = new Cell(source.X + offsetX, source.Y + offsetY);

        if (!grid.IsInside(target) || !grid.IsTree(target))
        {
            return;
        }

        var windEffect = windHelper.CalculateWindEffect(source, target);
        var distanceFalloff = MinSpotFireRadius / distanceToTarget;

        var chance =
            distanceFalloff *
            windEffect *
            cachedHumidityEffect *
            cachedTemperatureEffect;

        if (randomHelper.NextDouble() < chance)
        {
            toIgnite.Add(target);
        }
    }

    private double CalculateFireSpreadChance(Cell burningCell, Cell neighbor)
    {
        // Uncommented later when impl. diffrent ground types
        //if (simulationConfig.TerrainConfig.UseTerrainGeneration)
        //{
        // 🌍 TOPOGRAPHIE-LOGIK
        //var terrain = terrainGrid[neighbor.X, neighbor.Y];
        // ❌ kein Feuer auf Wasser oder Felsen
        //if (terrain.Type != TerrainType.Soil)
        //{
        //    return 0;
        //}
        //}

        var windEffect =
            windHelper.CalculateWindEffect(burningCell, neighbor);

        var chance =
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
            IgniteTree(burningCell);
        }
    }

    /// <summary>
    /// Spawnt die Feueranimation für eine Zelle.
    /// Bei extrem hoher Simulation-Geschwindigkeit (>= 40x) wird die Animation deaktiviert.
    /// </summary>
    /// <param name="cell">Die Zelle, die brennt</param>
    /// <param name="simulationSpeed">Die aktuelle Simulation-Geschwindigkeit</param>
    private void SpawnFireEffect(Cell burningCell)
    {
        // Bei sehr hoher Geschwindigkeit keine Animation abspielen
        const SimulationSpeed MaxSpeedForAnimation = SimulationSpeed.Ultra;
        if (simulationSpeed == MaxSpeedForAnimation)
        {
            return;
        }

        if (simulationConfig.VisualEffectsConfig.ShowFlameAnimations)
        {
            var fire = new FireAnimation(
                burningCell,
                ForestCanvas,
                simulationConfig.TreeConfig.Size
            );

            fireAnimations[burningCell] = fire;
            fire.Start();
        }

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
        grid.Clear(cell);
        burningTrees.Remove(cell);

        if (simulationConfig.VisualEffectsConfig.ShowFlameAnimations &&
            fireAnimations.TryGetValue(cell, out var fire))
        {
            fire.Stop();
            fireAnimations.Remove(cell);
        }

        if (treeElements.TryGetValue(cell, out var tree))
        {
            totalBurnedTrees++;
            UpdateGridForBurnedDownTree(cell, tree);
        }

        activeTrees.Remove(cell);
        growableCells.Add(cell);

        UpdateTreeUI();
    }

    private void UpdateGridForBurnedDownTree(Cell cell, Shape tree)
    {
        if (simulationConfig.VisualEffectsConfig.ShowBurnedDownTrees)
        {
            tree.Fill = Brushes.Gray;
            return;
        }

        ForestCanvas.Children.Remove(tree);
        treeElements.Remove(cell);
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
        for (var x = 0; x < grid.Cols; x++)
        {
            for (var y = 0; y < grid.Rows; y++)
            {
                // sanfter Verlauf: Zentrum = hoch, Ränder = niedrig
                var centerX = grid.Cols / 2.0;
                var centerY = grid.Rows / 2.0;

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

    private async Task IgniteRandomCell()
    {
        var minChanceToHitTree = CalculateCurrentTreeDensityPercent() / 100;

        var cell = GetCellByChance(minChanceToHitTree);

        if (simulationConfig.VisualEffectsConfig.ShowLightning)
        {
            await ShowLightning(cell);

            fireEventsHistory.Add(new(
                FireEventType.Lightning,
                accumulatedSimulationTime
            ));
        }

        IgniteTree(cell);
    }

    private Cell GetCellByChance(double minChanceToHitTree)
    {
        if (randomHelper.NextDouble() < minChanceToHitTree)
        {
            return randomHelper.NextCell(activeTrees);
        }

        return randomHelper.NextCell(grid.Cols, grid.Rows);
    }

    private async Task ShowLightning(Cell cell)
    {
        var lightningCell = CreateCellShape(cell, Colors.LightningColor);

        Canvas.SetLeft(lightningCell, cell.X * simulationConfig.TreeConfig.Size);
        Canvas.SetTop(lightningCell, cell.Y * simulationConfig.TreeConfig.Size);
        ForestCanvas.Children.Add(lightningCell);

        var boltEffect = CreateLightningBolt(cell);
        ForestCanvas.Children.Add(boltEffect);

        if (simulationConfig.VisualEffectsConfig.ShowBoltScreenFlash)
        {
            await FlashScreen();
        }
        await Task.Delay(millisecondsDelay: 80);

        ForestCanvas.Children.Remove(lightningCell);
        ForestCanvas.Children.Remove(boltEffect);
    }

    private async Task FlashScreen()
    {
        screenFlash.Opacity = 0.6;
        await Task.Delay(40);   // ~1 Frame
        screenFlash.Opacity = 0;
    }

    private Polyline CreateLightningBolt(Cell target)
    {
        var size = simulationConfig.TreeConfig.Size;

        var startX = target.X * size + size / 2f;
        var startY = 0f;

        var endX = startX;
        var endY = target.Y * size + size / 2f;

        var points = new PointCollection
        {
            new(startX, startY)
        };

        const byte boltSegments = 8;
        for (var i = 1; i < boltSegments; i++)
        {
            var t = i / (float)boltSegments;
            var x = startX + randomHelper.NextDouble(-15, 15);
            var y = startY + (endY - startY) * t;
            points.Add(new(x, y));
        }

        points.Add(new(endX, endY));

        return new Polyline
        {
            Points = points,
            Stroke = Brushes.LightBlue,
            StrokeThickness = 2.5,
            Opacity = 1
        };
    }

    private int CalculateMaxTreesPossible()
        => Math.Max(1, (int)(grid.Cols * grid.Rows * simulationConfig.TreeConfig.ForestDensity));

    private void UpdateTreeUI()
    {
        UpdateTreeDensityText?.Invoke(
            $"{activeTrees.Count} / {cachedMaxTreesPossible} ({CalculateCurrentTreeDensityPercent():F0}%)"
        );

        UpdateTotalGrownTreesText?.Invoke(totalGrownTrees.ToString());
        UpdateTotalBurnedTreesText?.Invoke(totalBurnedTrees.ToString());
    }

    private void UpdateWindUI()
    {
        var windStrength = windHelper.CurrentWindStrength;

        var windStrengthReadablePercent = windStrength * 100;
        var beaufortScale = (int)WindMapper.ConvertWindPercentStrenghToBeaufort(windStrength);

        UpdateWindStrengthText?.Invoke(
            $"{windStrengthReadablePercent:F0}% ({beaufortScale} Bft)"
        );
    }
}
