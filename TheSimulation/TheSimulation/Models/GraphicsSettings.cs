namespace TheSimulation;

/// <summary>
/// <see langword="new"/> <see langword="record"/> <see langword="class"/> representing graphics settings for the simulation.
/// Used to configure visual effects such as lightning, fire particles, and smoke particles.
/// Interfaces with the <see cref="MainWindow"/> and <see cref="GraphicsWindow"/> to visually apply user preferences.
/// By setting the settings here, the default settings are set for GraphicsWindow when it is first created.
/// </summary>
public sealed record GraphicsSettings
{
    public bool ShowLightning { get; set; } = true;
    public bool ShowBoltFlashes { get; set; } = false;
    public bool ShowFireParticles { get; set; } = true;
    public bool ShowSmokeParticles { get; set; } = true;
    public bool ShowFlamesOnTrees { get; set; } = true;
    public bool ShowBurnedDownTrees { get; set; } = false;
    public TreeShapeType TreeShape { get; set; } = SimulationDefaultsData.DefaultTreeShapeType;

    public GraphicsSettings() { }

    /// <summary>
    /// Resets all values to their initial defaults.
    /// Useful for the "Reset" button in the UI.
    /// </summary>
    public void ResetToDefaults()
    {
        // Weather effcts zurücksetzen
        ShowLightning = true;
        ShowBoltFlashes = false;

        // Fire zurücksetzen
        ShowFireParticles = true;

        // Particle effects zurücksetzen
        ShowSmokeParticles = true;
        ShowFlamesOnTrees = true;

        // Trees zurücksetzen
        ShowBurnedDownTrees = false;
        TreeShape = SimulationDefaultsData.DefaultTreeShapeType;
    }

    /// <summary>
    /// Creates a deep copy of the current settings.
    /// </summary>
    public GraphicsSettings Duplicate() => this with { };
}
