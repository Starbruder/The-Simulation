using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TheSimulation;

public static class UIHelper
{
    public static void InitializeWindowIcon(this Window window)
    {
        var iconUri = new Uri("pack://application:,,,/Assets/Icons/burning-tree-in-circle.ico");
        window.Icon = BitmapFrame.Create(iconUri);
    }

    public static void UpdateTimerUI(TextBlock target, DateTime simulationStartTime)
    {
        var elapsed = DateTime.Now - simulationStartTime;
        target.Text = $"Runtime: {elapsed:hh\\:mm\\:ss}";
    }
}
