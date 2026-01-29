using System.Windows;
using System.Windows.Media.Animation;

namespace TheSimulation;

public static class UIAnimationHelper
{
    public static void FadeOut(UIElement element, Action onComplete)
    {
        var fade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase
            {
                EasingMode = EasingMode.EaseOut
            }
        };

        fade.Completed += (s, e) =>
        {
            element.Visibility = Visibility.Collapsed;
            onComplete?.Invoke();
        };

        element.BeginAnimation(UIElement.OpacityProperty, fade);
    }
}
