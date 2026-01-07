using System.Windows.Controls;

namespace TheSimulation;

/// <summary>
/// Provides helper methods for updating timer-related user interface elements in a simulation application.
/// </summary>
public static class TimerVisualizer
{
	/// <summary>
	/// Updates the provided TextBlock with the elapsed time since the simulation started.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="simulationStartTime"></param>
	public static void UpdateTimerUI(TextBlock target, DateTime simulationStartTime)
    {
        var elapsed = DateTime.Now - simulationStartTime;
        target.Text = $"Runtime: {elapsed:hh\\:mm\\:ss}";
    }
}
