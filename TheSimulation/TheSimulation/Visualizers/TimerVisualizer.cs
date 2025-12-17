using System.Windows.Controls;

namespace TheSimulation;

public static class TimerVisualizer
{
    public static void UpdateTimerUI(TextBlock target, DateTime simulationStartTime)
    {
        var elapsed = DateTime.Now - simulationStartTime;
        target.Text = $"Runtime: {elapsed:hh\\:mm\\:ss}";
    }
}
