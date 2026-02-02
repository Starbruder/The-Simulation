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

        TreeShapeComboBox.ItemsSource = Enum.GetValues<TreeShapeType>();

        LightningCheckBox.IsChecked = settings.ShowLightning;
        BoltFlashCheckBox.IsChecked = settings.ShowBoltFlashes;
        FireSparksCheckBox.IsChecked = settings.ShowFireParticles;
        SmokeCheckBox.IsChecked = settings.ShowSmokeParticles;
        FlameAnimationsCheckBox.IsChecked = settings.ShowFlamesOnTrees;
        BurnedTreeCheckBox.IsChecked = settings.ShowBurnedDownTrees;
        TreeShapeComboBox.SelectedItem = settings.TreeShape;
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
        var selectedTreeShape = TreeShapeComboBox.SelectedItem;

        settings.ShowLightning = lightningEnabled;
        settings.ShowBoltFlashes = boltFlashesEnabled;
        settings.ShowFireParticles = fireSparksEnabled;
        settings.ShowSmokeParticles = smokeEnabled;
        settings.ShowFlamesOnTrees = flameAnimationsEnabled;
        settings.ShowBurnedDownTrees = showBurnedDownTrees;
        if (selectedTreeShape is TreeShapeType selectedShape)
        {
            settings.TreeShape = selectedShape;
        }

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
