namespace TheSimulation;

/// <summary>
/// Definiert die Einstellungen für die Geländegenerierung innerhalb der Simulation.
/// Bestimmt, welche Landschaftselemente beim Initialisieren der Welt erzeugt werden.
/// </summary>
/// <param name="UseTerrainGeneration">Gibt an, ob ein prozedurales Gelände (z. B. Höhenunterschiede) generiert werden soll.</param>
/// <param name="EnableWaterBodies">Bestimmt, ob Wasserflächen (Seen oder Flüsse) in der Simulation platziert werden.</param>
/// <param name="EnableRocks">Gibt an, ob unbrennbare Hindernisse wie Felsen oder Gesteinsformationen generiert werden sollen.</param>
public sealed record TerrainConfig
(
    bool UseTerrainGeneration,
    bool EnableWaterBodies,
    bool EnableRocks
);
