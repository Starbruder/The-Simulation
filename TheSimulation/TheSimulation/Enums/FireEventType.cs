namespace TheSimulation;

/// <summary>
/// Specifies the type of fire event that initiates or influences a fire within the simulation.
/// </summary>
/// <remarks>Use this enumeration to indicate the origin of a fire event, such as natural causes or manual
/// intervention. Additional event types may be introduced in future versions to support more fire initiation
/// scenarios.</remarks>
public enum FireEventType
{
    // TODO: In future versions map editor can place controlled burns
    //PlannedControlledBurn,
    //SelfIgnition,
    Lightning,
    ManualIgnition // via user interaction
}
