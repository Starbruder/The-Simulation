namespace TheSimulation;

public static class FormatHelper
{
    public static string FormatTreeDensityText(int activeTreeCount, double maxTreesPossible)
    {
        var density = activeTreeCount / maxTreesPossible;
        var densityPercent = (int)Math.Round(density * 100);

        return $"{activeTreeCount} / {maxTreesPossible} ({densityPercent}%)";
    }
}
