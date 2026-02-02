using System.Windows;
using System.Windows.Media.Animation;

namespace TheSimulation;

/// <summary>
/// Stellt Hilfsmethoden für Benutzeroberflächen-Animationen bereit.
/// Sorgt für flüssige Übergänge, um die Benutzererfahrung (UX) der Simulation zu verbessern.
/// </summary>
public static class UIAnimationHelper
{
    /// <summary>
    /// Blendet ein UI-Element sanft aus, indem die Deckkraft (Opacity) animiert wird.
    /// Nach Abschluss wird das Element vollständig aus dem Layout entfernt.
    /// </summary>
    /// <remarks>
    /// Die Animation nutzt eine quadratische Beschleunigungskurve (EaseOut), was natürlicher wirkt als ein linearer Übergang.
    /// Wichtig: Das Element wird am Ende auf <see cref="Visibility.Collapsed"/> gesetzt, damit es keine Mausereignisse mehr abfängt.
    /// </remarks>
    /// <param name="element">Das visuelle Element (z. B. das Pause-Overlay), das ausgeblendet werden soll.</param>
    /// <param name="onComplete">Eine optionale Rückruffunktion, die ausgeführt wird, sobald die Animation beendet ist.</param>
    public static void FadeOut(UIElement element, Action onComplete)
    {
        // 1. Definition der Animation: Von 100% Sichtbarkeit auf 0%
        var fade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300), // Kurze, knackige 300ms

            // Die EasingFunction sorgt dafür, dass die Animation nicht abrupt stoppt, 
            // sondern sanft "ausgleitet".
            EasingFunction = new QuadraticEase
            {
                EasingMode = EasingMode.EaseOut
            }
        };

        // 2. Der Aufräum-Prozess nach der Animation
        fade.Completed += (s, e) =>
        {
            // Erst wenn das Element unsichtbar ist, nehmen wir es aus dem Layoutfluss.
            // "Collapsed" bedeutet: Es braucht keinen Platz mehr und ist für die Maus "durchsichtig".
            element.Visibility = Visibility.Collapsed;

            // Falls der Aufrufer noch was vorhat (z.B. Simulation starten), jetzt ist der Moment.
            onComplete?.Invoke();
        };

        // 3. Startschuss: Wir animieren die Opacity-Eigenschaft des WPF-Elements.
        element.BeginAnimation(UIElement.OpacityProperty, fade);
    }
}
