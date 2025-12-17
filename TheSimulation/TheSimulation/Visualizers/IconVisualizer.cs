using System.Windows;
using System.Windows.Media.Imaging;

namespace TheSimulation;

public static class IconVisualizer
{
	public static void InitializeWindowIcon(this Window window)
	{
		var iconUri = new Uri("pack://application:,,,/Assets/Icons/burning-tree-in-circle.ico");
		window.Icon = BitmapFrame.Create(iconUri);
	}
}
