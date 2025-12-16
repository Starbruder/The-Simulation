using System.Windows;
using System.Windows.Media.Imaging;

namespace TheSimulation.UI;

public static class UIHelper
{
	public static void InitializeWindowIcon(this Window window)
	{
		var iconUri = new Uri("pack://application:,,,/Assets/Images/burning-tree-in-circle.ico");
		window.Icon = BitmapFrame.Create(iconUri);
	}
}
