namespace TheSimulation;

/// <summary>
/// <see langword="new"/> <see langword="class"/> representing graphics settings for the simulation.
/// Used to configure visual effects such as lightning, fire particles, and smoke particles.
/// Interfaces with the <see cref="MainWindow"/> and <see cref="GraphicsWindow"/> to visually apply user preferences.
/// </summary>
public sealed class GraphicsSettings
{
    public bool ShowLightning { get; set; } = true;
    public bool ShowBoltFlashes { get; set; } = false;
    public bool ShowFireParticles { get; set; } = true;
    public bool ShowSmokeParticles { get; set; } = true;
    public bool ShowBurnedDownTrees { get; set; } = false;

    public GraphicsSettings() { }

    public GraphicsSettings
    (
        bool showLightning,
        bool showBoltFlashes,
        bool showFireParticles,
        bool showSmokeParticles,
        bool showBurnedDownTrees
    )
    {
        ShowLightning = showLightning;
        ShowBoltFlashes = showBoltFlashes;
        ShowFireParticles = showFireParticles;
        ShowSmokeParticles = showSmokeParticles;
        ShowBurnedDownTrees = showBurnedDownTrees;
    }
}
