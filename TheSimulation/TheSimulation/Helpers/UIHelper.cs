using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TheSimulation;

public static class UIHelper
{
    public static void InitializeWindowIcon(this Window window)
    {
        var iconUri = new Uri("pack://application:,,,/Assets/Icons/burning-tree-in-circle.ico");
        window.Icon = BitmapFrame.Create(iconUri);
    }
}
