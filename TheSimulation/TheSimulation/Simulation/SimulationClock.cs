using System.Windows.Threading;

namespace TheSimulation;

/// <summary>
/// Repräsentiert eine Uhr bzw. einen Timer für die Steuerung verschiedener
/// zeitlicher Abläufe der Simulation, wie Wachstum, Feuer, Zündung, Wind und Sekundentakt.
/// </summary>
public sealed class SimulationClock
{
    /// <summary>
    /// Ereignis, das jede Sekunde ausgelöst wird.
    /// </summary>
    public event Action Tick1s;

    /// <summary>
    /// Ereignis für den Wachstums-Takt (Baumwachstum).
    /// </summary>
    public event Action GrowTick;

    /// <summary>
    /// Ereignis für den Feuer-Ausbreitungstakt.
    /// </summary>
    public event Action FireTick;

    /// <summary>
    /// Ereignis für das Zünd-Takt (z.B. zufällige Brandauslöser).
    /// </summary>
    public event Action IgniteTick;

    /// <summary>
    /// Ereignis für Windänderungen.
    /// </summary>
    public event Action WindTick;

    private readonly DispatcherTimer secondTimer = new();

    private readonly DispatcherTimer growTimer = new();

    private readonly DispatcherTimer fireTimer = new();

    private readonly DispatcherTimer igniteTimer = new();

    private readonly DispatcherTimer windTimer = new();

    /// <summary>
    /// Zeitintervall (in Millisekunden) für die Änderung der Windrichtung/-stärke.
    /// </summary>
    public const uint windChangeIntervalMs = 300 + (int)SimulationSpeed.Normal;

    /// <summary>
    /// Zeitspanne von genau einer Sekunde.
    /// </summary>
    public readonly TimeSpan TimerSecond = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="SimulationClock"/>-Klasse
    /// und verknüpft die Timer mit ihren jeweiligen Tick-Ereignissen.
    /// </summary>
    public SimulationClock()
    {
        secondTimer.Interval = TimerSecond;
        secondTimer.Tick += (_, _) => Tick1s?.Invoke();

        growTimer.Tick += (_, _) => GrowTick?.Invoke();
        fireTimer.Tick += (_, _) => FireTick?.Invoke();
        igniteTimer.Tick += (_, _) => IgniteTick?.Invoke();
        windTimer.Tick += (_, _) => WindTick?.Invoke();
    }

    /// <summary>
    /// Startet den Timer für Baumwachstum.
    /// </summary>
    public void StartGrowTimer() => growTimer.Start();

    /// <summary>
    /// Startet den Timer für Feuer-Ausbreitung.
    /// </summary>
    public void StartFireTimer() => fireTimer.Start();

    /// <summary>
    /// Startet den Timer für Brandzündung.
    /// </summary>
    public void StartIgniteTimer() => igniteTimer.Start();

    /// <summary>
    /// Startet den Timer für Windänderungen.
    /// </summary>
    public void StartWindTimer() => windTimer.Start();

    /// <summary>
    /// Startet den Timer, der jede Sekunde ein Ereignis auslöst.
    /// </summary>
    public void StartSecondTimer() => secondTimer.Start();

    /// <summary>
    /// Startet alle Timer (Wachstum, Feuer, Zündung, Wind und Sekundentakt).
    /// </summary>
    public void Start()
    {
        secondTimer.Start();
        growTimer.Start();
        fireTimer.Start();
        igniteTimer.Start();
        windTimer.Start();
    }

    /// <summary>
    /// Stoppt alle Timer.
    /// </summary>
    public void Stop()
    {
        secondTimer.Stop();
        growTimer.Stop();
        fireTimer.Stop();
        igniteTimer.Stop();
        windTimer.Stop();
    }

    /// <summary>
    /// Setzt die Geschwindigkeit der Simulation, d.h. die Intervalle der Timer.
    /// </summary>
    /// <param name="speed">Die gewünschte SimulationSpeed, die die Intervalle beeinflusst.</param>
    public void SetSpeed(SimulationSpeed speed)
    {
        var baseMs = (int)speed;

        growTimer.Interval = TimeSpan.FromMilliseconds(baseMs);
        fireTimer.Interval = TimeSpan.FromMilliseconds(baseMs);
        igniteTimer.Interval = TimeSpan.FromMilliseconds(baseMs * 750);
        windTimer.Interval = TimeSpan.FromMilliseconds(baseMs + windChangeIntervalMs);
    }
}
