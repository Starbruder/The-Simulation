namespace TheSimulation;

public static class FormatHelper
{
    public static string FormatTreeDensityText(int activeTreeCount, int maxTreesPossible)
    {
        var density = activeTreeCount / (float)maxTreesPossible;
        var densityPercent = (int)Math.Round(density * 100);

        return $"{activeTreeCount} / {maxTreesPossible} ({densityPercent}%)";
    }
}
