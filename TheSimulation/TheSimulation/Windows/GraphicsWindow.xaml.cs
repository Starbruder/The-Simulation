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
    }

    private void ApplyGraphicsSettings(object sender, RoutedEventArgs e)
    {
        var lightningEnabled = LightningCheckBox.IsChecked ?? false;
        var fireSparksEnabled = FireSparksCheckBox.IsChecked ?? false;
        var smokeEnabled = SmokeCheckBox.IsChecked ?? false;

        ApplySettings(lightningEnabled, fireSparksEnabled, smokeEnabled);
        Close();
    }

    private void ApplySettings(bool showLightning, bool showFireSparks, bool showSmoke)
    {
        settings.ShowLightning = showLightning;
        settings.ShowFireParticles = showFireSparks;
        settings.ShowSmokeParticles = showSmoke;
    }
}
