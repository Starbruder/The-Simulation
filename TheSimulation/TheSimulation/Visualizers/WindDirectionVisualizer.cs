using System.Windows;
using System.Windows.Controls;

namespace TheSimulation;

public static class WindDirectionVisualizer
{
    public static string GetWindDirectionVisual(float windAngleDegrees)
    {
        return windAngleDegrees switch
        {
            var angle when (angle >= 337.5 || angle < 22.5) => "North",
            var angle when (angle >= 22.5 && angle < 67.5) => "Northeast",
            var angle when (angle >= 67.5 && angle < 112.5) => "East",
            var angle when (angle >= 112.5 && angle < 157.5) => "Southeast",
            var angle when (angle >= 157.5 && angle < 202.5) => "South",
            var angle when (angle >= 202.5 && angle < 247.5) => "Southwest",
            var angle when (angle >= 247.5 && angle < 292.5) => "West",
            var angle when (angle >= 292.5 && angle < 337.5) => "Northwest",
            _ => "Unknown"
        };
    }

    //public static void UpdateWindUI(TextBlock target, Vector direction)
    //{
    //    if (direction.Length == 0)
    //    {
    //        target.Text = "Wind: Calm";
    //        return;
    //    }
    //    direction.Normalize();
    //    var angleRadians = Math.Atan2(direction.Y, direction.X);
    //    var angleDegrees = (float)((angleRadians * (180.0 / Math.PI) + 360) % 360);
    //    var directionText = GetWindDirectionVisual(angleDegrees);
    //    target.Text = $"Wind Direction: {directionText} ({angleDegrees:F1}°)";
    //}

    public static void UpdateWindUI(TextBlock target, Vector direction)
    {
        // Winkel anpassen: 0° = North, Uhrzeigersinn
        var angle = Math.Atan2(direction.X, direction.Y) * 180 / Math.PI;
        if (angle < 0)
        {
            angle += 360;
        }

        var snappedDir = WindMapper.ToWindDirection((float)angle);

        var directionName = snappedDir switch
        {
            WindDirection.North => "North",
            WindDirection.NorthEast => "Northeast",
            WindDirection.East => "East",
            WindDirection.SouthEast => "Southeast",
            WindDirection.South => "South",
            WindDirection.SouthWest => "Southwest",
            WindDirection.West => "West",
            WindDirection.NorthWest => "Northwest",
            _ => "Unknown"
        };

        target.Text = $"Wind direction: {directionName} ({angle:0}°)";
    }
}
