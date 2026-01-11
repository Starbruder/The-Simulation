namespace TheSimulation;

public sealed record FireEvent
(
    // position of the fire event if needed in future
    //Cell Position,
    FireEventType Type,
    TimeSpan SimulationTimestamp
);
