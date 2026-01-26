using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheSimulation;

/// <summary>
/// Repräsentiert den gesamten Simulationsprozess einer Waldbrand-Simulation.
/// </summary>
public sealed class ForestFireSimulation
{
    private Canvas ForestCanvas { get; }

    private readonly SimulationConfig simulationConfig;
    private readonly RandomHelper randomHelper;

    public required Action<string> UpdateSimulationTimeText;
    public required Action<string> UpdateTreeDensityText;
    public required Action<string> UpdateWindStrengthText;
    public required Action<string> UpdateTotalGrownTreesText;
    public required Action<string> UpdateTotalBurnedTreesText;

    private readonly WindHelper windHelper;
    private readonly WindCompassVisualizer windVisualizer;
    private readonly ParticleGenerator particleGenerator;
    private TimeSpan accumulatedSimulationTime = TimeSpan.Zero;

    private readonly SimulationClock clock = new();

    private ForestGrid grid = new(0, 0);

    private int cachedMaxTreesPossible;
    private float cachedTemperatureEffect;
    private float cachedHumidityEffect;

    public bool IsPaused { get; private set; } = false;
    private bool isFireActiveThenPause = false;

    private uint totalGrownTrees = 0;
    private uint totalBurnedTrees = 0;

    private SimulationSpeed simulationSpeed;

    private readonly Dictionary<Cell, Shape> treeElements = [];
    private readonly HashSet<Cell> activeTrees = [];
    private readonly HashSet<Cell> growableCells = [];
    private readonly HashSet<Cell> burningTrees = [];

    private readonly Dictionary<Cell, FireAnimation> fireAnimations = [];

    private readonly List<SimulationSnapshot> simulationHistory = [];

    private readonly List<FireEvent> fireEventsHistory = [];

    private TerrainCell[,] terrainGrid;

    private readonly Rectangle screenFlash = new();

    public ForestFireSimulation(SimulationConfig simulationConfig, RandomHelper random, Canvas ForestCanvas, SimulationSpeed simulationSpeed = SimulationSpeed.Normal)
    {
        // To get rid of the warning CS8618
        terrainGrid = new TerrainCell[0, 0];

        this.ForestCanvas = ForestCanvas;
        this.randomHelper = random;
        this.simulationConfig = simulationConfig;
        this.simulationSpeed = simulationSpeed;
        windHelper = new(simulationConfig.EnvironmentConfig.WindConfig);
        windVisualizer =
            new(ForestCanvas, simulationConfig.EnvironmentConfig.WindConfig, windHelper);

        particleGenerator = new ParticleGenerator(ForestCanvas);

        ForestCanvas.Loaded += async (_, _) => await InitializeSimulationAsync();

        ForestCanvas.PreviewMouseDown += (_, e) =>
        {
            MouseClick(e);
        };

        StartOrResumeSimulation();
    }

    private Cell GetMouseClickedCell(MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(ForestCanvas);

        var x = (int)(pos.X / simulationConfig.TreeConfig.Size);
        var y = (int)(pos.Y / simulationConfig.TreeConfig.Size);

        return new(x, y);
    }

    private void MouseClick(MouseButtonEventArgs e)
    {
        var cell = GetMouseClickedCell(e);

        if (IsOutOfBoundsClickOfCanvas(cell))
        {
            return;
        }

        switch (e.ChangedButton)
        {
            case MouseButton.Left:
                MouseBurnClick(cell);
                break;

            case MouseButton.Middle:
                MouseDestoryClick(cell);
                break;

            case MouseButton.Right:
                MouseGrowClick(cell);
                break;

            case MouseButton.XButton1: // Hier könnte man noch andere Funktionen hinzufügen
                break;
            case MouseButton.XButton2: // Z.b: Zoom In/Out oder Simulationsgeschwindigkeit ändern
                break;
        }
    }

    /// <summary>
    /// Löst das Problem, dass außerhalb geklickt wird und das Programm abstürzt.
    /// of out of bounds (Knapp außerhalb des Baumrasters klicken)
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    private bool IsOutOfBoundsClickOfCanvas(Cell cell) => !grid.IsInside(cell);

    private void MouseGrowClick(Cell cell)
    {
        var isTreeThere = grid.IsTree(cell);
        var isTreeBurning = grid.IsBurning(cell);

        if (isTreeThere || isTreeBurning)
        {
            return;
        }

        if (!growableCells.Contains(cell))
        {
            return;
        }

        ResetCellState(cell);

        if (simulationConfig.TerrainConfig.UseTerrainGeneration)
        {
            var terrain = terrainGrid[cell.X, cell.Y];

            if (terrain.Type != TerrainType.Soil)
            {
                return;
            }
        }

        AddTree(cell);
    }

    private void MouseDestoryClick(Cell cell)
    {
        if (grid.IsEmpty(cell))
        {
            return;
        }

        totalGrownTrees--;
        DestroyCell(cell);
    }

    private void DestroyCell(Cell cell)
    {
        ResetCellState(cell);
        UpdateTreeUI();
    }

    private void ResetCellState(Cell cell)
    {
        RemoveCellAndFireAnimation(cell);

        grid.Clear(cell);

        RemoveTree(cell);

        activeTrees.Remove(cell);
        growableCells.Add(cell);
    }

    private void RemoveCellAndFireAnimation(Cell cell)
    {
        if (burningTrees.Remove(cell))
        {
            if (fireAnimations.TryGetValue(cell, out var fire))
            {
                fire.Stop();
                fireAnimations.Remove(cell);
            }
        }
    }

    private void RemoveTree(Cell cell)
    {
        if (treeElements.TryGetValue(cell, out var tree))
        {
            ForestCanvas.Children.Remove(tree);
            treeElements.Remove(cell);
        }
    }

    public void StartOrResumeSimulation()
    {
        IsPaused = false;

        if (simulationConfig.TreeConfig.AllowRegrowForest)
        {
            clock.StartGrowTimer();
        }

        clock.StartIgniteTimer();
        clock.StartFireTimer();
        clock.StartSecondTimer();

        if (simulationConfig.EnvironmentConfig.WindConfig.RandomDirection ||
            simulationConfig.EnvironmentConfig.WindConfig.RandomStrength)
        {
            clock.StartWindTimer();
            return;
        }

        windVisualizer.Draw();
        UpdateWindUI();
    }

    private void MouseBurnClick(Cell cell)
    {
        IgniteTree(cell);
        fireEventsHistory.Add(new(
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
        InitializeSimulationClock();
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
            clock.GrowTick += GrowStep;
        }
        clock.IgniteTick += async () => await IgniteRandomCell();
        clock.FireTick += FireStep;

        SetSimulationSpeed(simulationSpeed);

        if (simulationConfig.EnvironmentConfig.WindConfig.RandomDirection ||
            simulationConfig.EnvironmentConfig.WindConfig.RandomStrength)
        {
            clock.WindTick += UpdateWind;
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

    private void InitializeSimulationClock()
    {
        var hours = (int)accumulatedSimulationTime.TotalHours;
        UpdateSimulationTimeText?.Invoke(
            $"Runtime: {hours:D2}:{accumulatedSimulationTime.Minutes:D2}:{accumulatedSimulationTime.Seconds:D2}"
        );

        clock.Tick1s += OnSimulationTick;
    }

    private void OnSimulationTick()
    {
        const int maxSimulationHours = 99;
        if (CalculateSimulationTime() >= new TimeSpan(maxSimulationHours, 0, 0)
        || simulationConfig.PrefillConfig.ShouldPrefillMap && LowDensityMinimumReached()
        && !simulationConfig.TreeConfig.AllowRegrowForest)
        {
            StopOrPauseSimulation();
            OpenEvalualtionWindow();
            return;
        }

        accumulatedSimulationTime = accumulatedSimulationTime.Add(clock.TimerSecond);

        var hours = (int)accumulatedSimulationTime.TotalHours;
        UpdateSimulationTimeText?.Invoke(
            $"Runtime: {hours:D2}:{accumulatedSimulationTime.Minutes:D2}:{accumulatedSimulationTime.Seconds:D2}"
        );
        RecordSimulationStats();
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
        IsPaused = true;
        clock.Stop();
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

    private void UpdateWind()
    {
        windHelper.RandomizeAndUpdateWind(); // Winkel und Strengh randomisieren
        var vector = windHelper.GetWindVector();
        windVisualizer.Update(vector);

        UpdateWindUI();
    }

    public void SetSimulationSpeed(SimulationSpeed simulationSpeed)
    {
        this.simulationSpeed = simulationSpeed;

        clock.SetSpeed(simulationSpeed);

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

    /// <summary>
    /// Simulate different tree types by using different colors
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    private Brush GetTreeColor(Cell cell)
    {
        var color = randomHelper.NextTreeColor();

        if (simulationConfig.TerrainConfig.UseTerrainGeneration)
        {
            // 🌍 TOPOGRAPHIE-COLOR-LOGIK
            var elevation = terrainGrid[cell.X, cell.Y].Elevation;
            return ColorHelper.AdjustColorByElevation(color, elevation);
        }

        return color;
    }

    /// <summary>
    /// Führt einen einzelnen Simulationsschritt für die Brandausbreitung aus.
    /// </summary>
    /// <remarks>
    /// In diesem Schritt werden alle aktuell brennenden Zellen ausgewertet,
    /// ohne den zugrunde liegenden Brandzustand während der Iteration zu verändern.
    ///
    /// Für jede brennende Zelle werden dabei folgende Aktionen durchgeführt:
    /// <list type="number">
    /// <item>
    /// <description>
    /// Es wird mit einer konfigurierbaren Wahrscheinlichkeit versucht,
    /// einen sogenannten Spot-Fire (Funkenflug) in größerer Entfernung auszulösen.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Es wird geprüft, ob sich das Feuer auf direkt benachbarte Zellen ausbreitet.
    /// Die Ausbreitungswahrscheinlichkeit wird durch Umwelteinflüsse wie Wind,
    /// Temperatur, Luftfeuchtigkeit und Topographie beeinflusst.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Die aktuell brennende Zelle wird zum Abbrennen markiert und nach Abschluss
    /// des Simulationsschritts aus dem aktiven Brandzustand entfernt.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// Die tatsächliche Zustandsänderung (Entzünden neuer Zellen und Abbrennen
    /// bestehender Brandzellen) erfolgt gesammelt nach der Auswertung aller
    /// aktuell brennenden Zellen, um Seiteneffekte während der Iteration zu vermeiden.
    ///
    /// Wird in diesem Schritt kein aktiver Brand mehr festgestellt, wird der
    /// Feuerzustand der Simulation entsprechend aktualisiert.
    /// </remarks>
    private void FireStep()
    {
        if (burningTrees.Count == 0)
        {
            isFireActiveThenPause = false;
            return;
        }

        var toIgnite = new HashSet<Cell>();
        var toBurnDown = new List<Cell>();

        foreach (var burningCell in burningTrees)
        {
            if (randomHelper.NextDouble() < simulationConfig.FireConfig.SpreadChancePercent / 100)
            {
                TryIgniteNearbyCell(burningCell, toIgnite);
            }

            foreach (var neighbor in grid.GetNeighbors(burningCell))
            {
                if (!grid.IsTree(neighbor) || grid.IsBurning(neighbor))
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

    /// <summary>
    /// Briefly flashes the screen by temporarily increasing the opacity of the screen overlay for ~1 Frame.
    /// </summary>
    /// <remarks>This method is intended to provide a quick visual feedback effect. It should be awaited to
    /// ensure the flash completes before proceeding with subsequent UI updates.</remarks>
    /// <returns>A task that represents the asynchronous flash operation.</returns>
    private async Task FlashScreen()
    {
        screenFlash.Opacity = 0.6;
        await Task.Delay(40);
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
