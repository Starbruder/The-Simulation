using System.Windows;

namespace TheSimulation;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // 🛡️ Extra-Schutz (empfohlen)
    // Globaler Exception-Handler(zeigt MessageBox statt App-Crash) :
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (sender, args) =>
        {
            MessageBox.Show(
                args.Exception.ToString(),
                "Unerwarteter Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            args.Handled = true; // verhindert, dass die App schließt
        };
    }
}
