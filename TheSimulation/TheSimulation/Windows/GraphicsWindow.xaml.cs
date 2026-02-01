using System.Windows;
using System.Windows.Input;

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
            TreeShapeType.Ellipse => 0,
            TreeShapeType.Rectangle => 1,
            _ => 0
        };
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.R)
        {
            ResetAllSettings_Click(sender, e);
            e.Handled = true;
        }
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
            0 => TreeShapeType.Ellipse,
            1 => TreeShapeType.Rectangle,
            _ => SimulationDefaultsData.DefaultTreeShapeType
        };

        Close();
    }

    private void ResetAllSettings_Click(object sender, RoutedEventArgs e)
    {
        // Weather effcts zurücksetzen
        LightningCheckBox.IsChecked = true;
        BoltFlashCheckBox.IsChecked = false;

        // Fire zurücksetzen
        FlameAnimationsCheckBox.IsChecked = true;

        // Particle effects zurücksetzen
        FireSparksCheckBox.IsChecked = true;
        SmokeCheckBox.IsChecked = true;

        // Trees zurücksetzen
        BurnedTreeCheckBox.IsChecked = false;
        TreeShapeComboBox.SelectedIndex = 0;
    }
}
