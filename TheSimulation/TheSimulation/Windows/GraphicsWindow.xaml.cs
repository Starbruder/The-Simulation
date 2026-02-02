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

        LoadGraphicsSettingsIntoUI();
    }

    private void LoadGraphicsSettingsIntoUI()
    {
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
        settings.ShowLightning = LightningCheckBox.IsChecked ?? true;
        settings.ShowBoltFlashes = BoltFlashCheckBox.IsChecked ?? false;
        settings.ShowFireParticles = FireSparksCheckBox.IsChecked ?? true;
        settings.ShowSmokeParticles = SmokeCheckBox.IsChecked ?? true;
        settings.ShowFlamesOnTrees = FlameAnimationsCheckBox.IsChecked ?? true;
        settings.ShowBurnedDownTrees = BurnedTreeCheckBox.IsChecked ?? false;

        if (TreeShapeComboBox.SelectedItem is TreeShapeType selectedShape)
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
        TreeShapeComboBox.SelectedItem = SimulationDefaultsData.DefaultTreeShapeType;
    }
}
