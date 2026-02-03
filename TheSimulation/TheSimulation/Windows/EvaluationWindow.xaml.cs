using Microsoft.Win32;
using System.Windows;

namespace TheSimulation;

public sealed partial class EvaluationWindow : Window
{
    // Wir halten eine Referenz auf das ViewModel statt nur auf das Record
    private readonly EvaluationViewModel viewModel;

    public EvaluationWindow(Evaluation data)
    {
        InitializeComponent();
        IconVisualizer.InitializeWindowIcon(this);

        // 1. ViewModel instanziieren
        viewModel = new(data);

        // 2. ViewModel als DataContext setzen (ermöglicht Binding in XAML)
        DataContext = viewModel;

        // 3. Charts zeichnen (OxyPlot braucht oft noch direkten Zugriff im Code-Behind)
        SetupCharts();
    }

    private void SetupCharts()
    {
        if (viewModel.HistoryCount < 1)
        {
            MessageBox.Show("Havent yet collected enough infos to display charts.",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Loaded += (_, _) =>
        {
            // Wir holen uns die fertig berechneten Models aus dem ViewModel
            GrownBurnedPlot.Model = viewModel.GrownBurnedModel;
            ActiveTreesPlot.Model = viewModel.ActiveTreesModel;
        };
    }

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        // Zugriff über das ViewModel
        if (viewModel.HistoryCount == 0)
        {
            MessageBox.Show("No simulation data available to export.", "Export failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Export simulation evaluation",
            Filter = "CSV files (*.csv)|*.csv",
            FileName = $"ForestFireEvaluation_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                // Nutze das im ViewModel gekapselte Data-Objekt
                EvaluationExporter.ExportCsv(viewModel.Data, dialog.FileName);
                MessageBox.Show("CSV export completed successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Export error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
