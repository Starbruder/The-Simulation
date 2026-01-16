using System.Windows.Controls;

namespace TheSimulation;

/// <summary>
/// Provides helper methods for updating timer-related user interface elements in a simulation application.
/// </summary>
public static class TimerVisualizer
{
    /// <summary>
    /// Updates the provided TextBlock with a formatted string of the elapsed simulation time.
    /// </summary>
    /// <param name="target">The TextBlock to update.</param>
    /// <param name="elapsed">The accumulated simulation time.</param>
    public static void UpdateTimerUI(TextBlock target, TimeSpan elapsed)
    {
        // We use TotalHours to ensure that if the simulation goes over 24h, 
        // the display doesn't reset to 00.
        int hours = (int)elapsed.TotalHours;
        target.Text = $"Runtime: {hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
    }
}
