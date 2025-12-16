using System.Windows;
using TheSimulation.UI;

namespace TheSimulation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        UIHelper.InitializeWindowIcon(this);
    }

    private void StartSimulation_Click(object sender, RoutedEventArgs e)
    {
		var simulationWindow = new SimulationWindow();
		simulationWindow.Show();
	}
}
