using System.Windows;
using System.Windows.Shapes;

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
        FlameAnimationsCheckBox.IsChecked = settings.ShowFlamesOnTrees;
        BurnedTreeCheckBox.IsChecked = settings.ShowBurnedDownTrees;
        TreeShapeComboBox.SelectedIndex = settings.TreeShape switch
        {
            Ellipse => 0,
            Rectangle => 1,
            _ => 0
        };
    }

    private void ApplyGraphicsSettings(object sender, RoutedEventArgs e)
    {
        var lightningEnabled = LightningCheckBox.IsChecked ?? true;
        var boltFlashesEnabled = BoltFlashCheckBox.IsChecked ?? false;
        var fireSparksEnabled = FireSparksCheckBox.IsChecked ?? true;
        var smokeEnabled = SmokeCheckBox.IsChecked ?? true;
        var flameAnimationsEnabled = FlameAnimationsCheckBox.IsChecked ?? true;
        var showBurnedDownTrees = BurnedTreeCheckBox.IsChecked ?? false;
        var selectedTreeShapeIndex = TreeShapeComboBox.SelectedIndex;

        settings.ShowLightning = lightningEnabled;
        settings.ShowBoltFlashes = boltFlashesEnabled;
        settings.ShowFireParticles = fireSparksEnabled;
        settings.ShowSmokeParticles = smokeEnabled;
        settings.ShowFlamesOnTrees = flameAnimationsEnabled;
        settings.ShowBurnedDownTrees = showBurnedDownTrees;
        settings.TreeShape = selectedTreeShapeIndex switch
        {
            0 => new Ellipse(),
            1 => new Rectangle(),
            _ => new Ellipse()
        };

        Close();
    }
}
