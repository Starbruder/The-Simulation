using System.Windows;
using System.Windows.Media.Imaging;

namespace TheSimulation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeWindowIcon();
    }

    private void InitializeWindowIcon()
    {
        var iconUri = new Uri("pack://application:,,,/Assets/Images/burning-tree-in-circle.ico");
        Icon = BitmapFrame.Create(iconUri);
    }

    private void StartSimulation_Click(object sender, RoutedEventArgs e)
    {
		var simulationWindow = new SimulationWindow();
		simulationWindow.Show();
	}
}
