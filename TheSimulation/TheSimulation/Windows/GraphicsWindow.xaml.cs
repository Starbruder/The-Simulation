using System.Windows;

namespace TheSimulation;

/// <summary>
/// Interaktionslogik für GraphicsSettingsWindow.xaml
/// </summary>
public sealed partial class GraphicsWindow : Window
{
    private readonly GraphicsSettings settings;

    public GraphicsWindow(GraphicsSettings settings)
    {
        this.settings = settings;

        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);

        LightningCheckBox.IsChecked = settings.ShowLightning;
        FireSparksCheckBox.IsChecked = settings.ShowFireParticles;
        SmokeCheckBox.IsChecked = settings.ShowSmokeParticles;
        BurnedTreeCheckBox.IsChecked = settings.ShowBurnedDownTrees;
    }

    private void ApplyGraphicsSettings(object sender, RoutedEventArgs e)
    {
        var lightningEnabled = LightningCheckBox.IsChecked ?? true;
        var fireSparksEnabled = FireSparksCheckBox.IsChecked ?? true;
        var smokeEnabled = SmokeCheckBox.IsChecked ?? true;
        var showBurnedDownTrees = BurnedTreeCheckBox.IsChecked ?? false;

        ApplySettings(lightningEnabled, fireSparksEnabled, smokeEnabled, showBurnedDownTrees);
        Close();
    }

    private void ApplySettings
        (bool showLightning, bool showFireSparks, bool showSmoke, bool showBurnedDownTrees)
    {
        settings.ShowLightning = showLightning;
        settings.ShowFireParticles = showFireSparks;
        settings.ShowSmokeParticles = showSmoke;
        settings.ShowBurnedDownTrees = showBurnedDownTrees;
    }
}
