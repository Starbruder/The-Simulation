using System.Windows;
using System.Windows.Media.Imaging;

namespace TheSimulation;

/// <summary>
/// Provides extension methods for setting the application window icon to a predefined image.
/// </summary>
/// <remarks>
/// This class is intended for use with WPF applications.
/// The icon resource must be present at the
/// specified path within the application's resources for the methods to function correctly.
/// </remarks>
public static class IconVisualizer
{
	/// <summary>
	/// Sets the window icon to a predefined application icon.
	/// </summary>
	/// <remarks>
	/// This method assigns the icon located at '/Assets/Icons/burning-tree-in-circle.ico' as the window's icon.
	/// Use this extension method to ensure a consistent application icon across windows.
	/// </remarks>
	/// <param name="window">
	/// The window whose icon is to be initialized. Cannot be null.
	/// </param>
	public static void InitializeWindowIcon(this Window window)
	{
		var iconUri = new Uri("pack://application:,,,/Assets/Icons/burning-tree-in-circle.ico");
		window.Icon = BitmapFrame.Create(iconUri);
	}
}
