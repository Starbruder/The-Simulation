namespace TheSimulation;

/// <summary>
/// Represents configuration options for enabling or disabling visual effects such as lightning, fire particles, and
/// smoke particles.
/// </summary>
/// <param name="ShowLightning"> A value indicating whether lightning effects are displayed. Set to <see langword="true"/> to show lightning;
/// otherwise, <see langword="false"/>.</param>
/// <param name="ShowBoltScreenFlash"> A value indicating whether screen flash effects for lightning bolts are displayed. Set to <see langword="true"/> to show flash;
/// otherwise, <see langword="false"/>.</param>
/// <param name="ShowFireParticles">A value indicating whether fire particle effects are displayed. Set to <see langword="true"/> to show fire
/// particles; otherwise, <see langword="false"/>.</param>
/// <param name="ShowSmokeParticles">A value indicating whether smoke particle effects are displayed. Set to <see langword="true"/> to show smoke
/// particles; otherwise, <see langword="false"/>.</param>
/// <param name="ShowBurnedDownTrees">A value indicating whether burned down trees are displayed. Set to <see langword="true"/> to show burned down
public sealed record VisualEffectsConfig
(
    bool ShowLightning,
    bool ShowBoltScreenFlash,
    bool ShowFireParticles,
    bool ShowSmokeParticles,
    bool ShowBurnedDownTrees
);
