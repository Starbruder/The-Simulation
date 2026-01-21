using System.Windows.Threading;

namespace TheSimulation;

public sealed class SimulationClock
{
    public event Action Tick1s;
    public event Action GrowTick;
    public event Action FireTick;
    public event Action IgniteTick;
    public event Action WindTick;

    private readonly DispatcherTimer secondTimer = new();
    private readonly DispatcherTimer growTimer = new();
    private readonly DispatcherTimer fireTimer = new();
    private readonly DispatcherTimer igniteTimer = new();
    private readonly DispatcherTimer windTimer = new();

    public const uint windChangeIntervalMs = 300 + (int)SimulationSpeed.Normal;

    public readonly TimeSpan TimerSecond = TimeSpan.FromSeconds(1);

    public SimulationClock()
    {
        secondTimer.Interval = TimerSecond;
        secondTimer.Tick += (_, _) => Tick1s?.Invoke();

        growTimer.Tick += (_, _) => GrowTick?.Invoke();
        fireTimer.Tick += (_, _) => FireTick?.Invoke();
        igniteTimer.Tick += (_, _) => IgniteTick?.Invoke();
        windTimer.Tick += (_, _) => WindTick?.Invoke();
    }

    public void StartGrowTimer() => growTimer.Start();

    public void StartFireTimer() => fireTimer.Start();

    public void StartIgniteTimer() => igniteTimer.Start();

    public void StartWindTimer() => windTimer.Start();

    public void StartSecondTimer() => secondTimer.Start();

    public void Start()
    {
        secondTimer.Start();
        growTimer.Start();
        fireTimer.Start();
        igniteTimer.Start();
        windTimer.Start();
    }

    public void Stop()
    {
        secondTimer.Stop();
        growTimer.Stop();
        fireTimer.Stop();
        igniteTimer.Stop();
        windTimer.Stop();
    }

    public void SetSpeed(SimulationSpeed speed)
    {
        var baseMs = (int)speed;

        growTimer.Interval = TimeSpan.FromMilliseconds(baseMs);
        fireTimer.Interval = TimeSpan.FromMilliseconds(baseMs);
        igniteTimer.Interval = TimeSpan.FromMilliseconds(baseMs * 750);
        windTimer.Interval = TimeSpan.FromMilliseconds(baseMs + windChangeIntervalMs);
    }
}
