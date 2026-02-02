namespace TheSimulation;

/// <summary>
/// Repräsentiert ein einzelnes, zeitlich datiertes Ereignis innerhalb der Brandlogik.
/// Dient als Datenbasis für die nachträgliche Auswertung und statistische Analyse der Simulation.
/// </summary>
/// <remarks>
/// Da es sich um ein <see langword="record"/> handelt, sind die Daten unveränderlich (immutable).
/// </remarks>
/// <param name="Type">Die Art des Ereignisses (z. B. Entzündung, Erlöschen, Baum verbrannt).</param>
/// <param name="SimulationTimestamp">Der exakte Zeitpunkt innerhalb der Simulationszeit, an dem das Ereignis auftrat.</param>
public sealed record FireEvent
(
    // position of the fire event if needed in future
    //Cell Position,
    FireEventType Type,
    TimeSpan SimulationTimestamp
);
