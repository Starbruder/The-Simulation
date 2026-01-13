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
        BoltFlashCheckBox.IsChecked = settings.ShowBoltFlashes;
        FireSparksCheckBox.IsChecked = settings.ShowFireParticles;
        SmokeCheckBox.IsChecked = settings.ShowSmokeParticles;
        BurnedTreeCheckBox.IsChecked = settings.ShowBurnedDownTrees;
    }

    private void ApplyGraphicsSettings(object sender, RoutedEventArgs e)
    {
        var lightningEnabled = LightningCheckBox.IsChecked ?? true;
        var boltFlashesEnabled = BoltFlashCheckBox.IsChecked ?? false;
        var fireSparksEnabled = FireSparksCheckBox.IsChecked ?? true;
        var smokeEnabled = SmokeCheckBox.IsChecked ?? true;
        var showBurnedDownTrees = BurnedTreeCheckBox.IsChecked ?? false;

        settings.ShowLightning = lightningEnabled;
        settings.ShowBoltFlashes = boltFlashesEnabled;
        settings.ShowFireParticles = fireSparksEnabled;
        settings.ShowSmokeParticles = smokeEnabled;
        settings.ShowBurnedDownTrees = showBurnedDownTrees;

        Close();
    }
}
