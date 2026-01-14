namespace TheSimulation;

/// <summary>
/// Specifies the primary compass directions used to indicate wind direction.
/// (clockwise, 0° = North)
/// </summary>
/// <remarks>
/// The values correspond to the four cardinal points on a compass, measured in degrees clockwise from North.
/// Use this enumeration to represent wind direction in meteorological or environmental applications.
/// </remarks>
public enum WindDirection
{
    North = 0,
    NorthEast = 45,
    East = 90,
    SouthEast = 135,
    South = 180,
    SouthWest = 225,
    West = 270,
    NorthWest = 315
}
