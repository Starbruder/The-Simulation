using System.Windows.Shapes;

namespace TheSimulation;

/// <summary>
/// <see langword="new"/> <see langword="class"/> representing graphics settings for the simulation.
/// Used to configure visual effects such as lightning, fire particles, and smoke particles.
/// Interfaces with the <see cref="MainWindow"/> and <see cref="GraphicsWindow"/> to visually apply user preferences.
/// By setting the settings here, the default settings are set for GraphicsWindow when it is first created.
/// </summary>
public sealed class GraphicsSettings
{
    public bool ShowLightning { get; set; } = true;
    public bool ShowBoltFlashes { get; set; } = false;
    public bool ShowFireParticles { get; set; } = true;
    public bool ShowSmokeParticles { get; set; } = true;
    public bool ShowFlamesOnTrees { get; set; } = true;
    public bool ShowBurnedDownTrees { get; set; } = false;
    public Shape TreeShape { get; set; } = new Ellipse();

    public GraphicsSettings() { }

    public GraphicsSettings
    (
        bool showLightning,
        bool showBoltFlashes,
        bool showFireParticles,
        bool showSmokeParticles,
        bool showFlamesOnTrees,
        bool showBurnedDownTrees,
        Shape treeShape
    )
    {
        ShowLightning = showLightning;
        ShowBoltFlashes = showBoltFlashes;
        ShowFireParticles = showFireParticles;
        ShowSmokeParticles = showSmokeParticles;
        ShowFlamesOnTrees = showFlamesOnTrees;
        ShowBurnedDownTrees = showBurnedDownTrees;
        TreeShape = treeShape;
    }
}
