using System.Windows;
using System.Windows.Media;

namespace TheSimulation;

public sealed class VisualHost : FrameworkElement
{
    private readonly VisualCollection visuals;

    public VisualHost()
    {
        visuals = new(this);
    }

    public void AddVisual(Visual visual) => visuals.Add(visual);

    protected override int VisualChildrenCount => visuals.Count;

    protected override Visual GetVisualChild(int index)
        => visuals[index];
}
