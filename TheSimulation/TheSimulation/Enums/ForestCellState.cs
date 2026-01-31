namespace TheSimulation;

/// <summary>
/// Specifies the possible states of a cell within a forest simulation, such as in a fire spread model.
/// </summary>
/// <remarks>Use this enumeration to represent whether a cell is empty, contains a tree, is currently burning, or
/// has already burned. The values can be used to track and update the state of each cell as the simulation
/// progresses.</remarks>
public enum ForestCellState
{
    Empty,     // kein Baum
    Tree,      // Baum
    Burning,   // brennt
    Burned     // verbrannt
}
