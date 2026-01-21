namespace TheSimulation;

/// <summary>
/// Repräsentiert das zweidimensionale Gitter (Grid) des Waldes,
/// in dem jede Zelle den Zustand eines Waldabschnitts speichert.
/// </summary>
/// <param name="cols">Anzahl der Spalten im Raster.</param>
/// <param name="rows">Anzahl der Zeilen im Raster.</param>
public sealed class ForestGrid(int cols, int rows)
{
    /// <summary>
    /// Anzahl der Spalten im Raster.
    /// </summary>
    public int Cols { get; } = cols;

    /// <summary>
    /// Anzahl der Zeilen im Raster.
    /// </summary>
    public int Rows { get; } = rows;

    private readonly ForestCellState[,] cells = new ForestCellState[cols, rows];

    /// <summary>
    /// Ermöglicht den Zugriff auf den Zustand einer Zelle im Raster anhand ihrer Koordinaten.
    /// </summary>
    /// <param name="c">Die Zelle (mit X- und Y-Koordinaten).</param>
    /// <returns>Der Zustand der Zelle (z.B. Baum, brennend, leer).</returns>
    public ForestCellState this[Cell c]
    {
        get => cells[c.X, c.Y];
        set => cells[c.X, c.Y] = value;
    }

    /// <summary>
    /// Setzt eine Zelle auf den Zustand 'Baum'.
    /// </summary>
    /// <param name="c">Die zu ändernde Zelle.</param>
    public void SetTree(Cell c) => this[c] = ForestCellState.Tree;

    /// <summary>
    /// Setzt eine Zelle auf den Zustand 'Brennend'.
    /// </summary>
    /// <param name="c">Die zu ändernde Zelle.</param>
    public void SetBurning(Cell c) => this[c] = ForestCellState.Burning;

    /// <summary>
    /// Setzt eine Zelle auf den Zustand 'Leer'.
    /// </summary>
    /// <param name="c">Die zu ändernde Zelle.</param>
    public void Clear(Cell c) => this[c] = ForestCellState.Empty;

    /// <summary>
    /// Prüft, ob die angegebene Zelle innerhalb des Gitters liegt.
    /// </summary>
    /// <param name="c">Die zu prüfende Zelle.</param>
    /// <returns>True, wenn die Zelle gültige Koordinaten im Raster hat, sonst False.</returns>
    public bool IsInside(Cell c)
        => c.X >= 0 && c.Y >= 0 && c.X < Cols && c.Y < Rows;

    /// <summary>
    /// Prüft, ob die angegebene Zelle einen Baum enthält.
    /// </summary>
    /// <param name="c">Die zu prüfende Zelle.</param>
    /// <returns>True, wenn ein Baum vorhanden ist, sonst False.</returns>
    public bool IsTree(Cell c)
        => IsInside(c) && this[c] == ForestCellState.Tree;

    /// <summary>
    /// Prüft, ob die angegebene Zelle brennt.
    /// </summary>
    /// <param name="c">Die zu prüfende Zelle.</param>
    /// <returns>True, wenn die Zelle brennt, sonst False.</returns>
    public bool IsBurning(Cell c)
        => IsInside(c) && this[c] == ForestCellState.Burning;

    /// <summary>
    /// Prüft, ob die angegebene Zelle leer ist.
    /// </summary>
    /// <param name="c">Die zu prüfende Zelle.</param>
    /// <returns>True, wenn die Zelle leer ist, sonst False.</returns>
    public bool IsEmpty(Cell c)
        => IsInside(c) && this[c] == ForestCellState.Empty;

    /// <summary>
    /// Liefert alle gültigen Nachbarzellen (inklusive Diagonalen) der angegebenen Zelle.
    /// Die Zelle selbst wird ausgeschlossen.
    /// </summary>
    /// <param name="cell">Die Zelle, deren Nachbarn ermittelt werden sollen.</param>
    /// <returns>Eine Aufzählung der benachbarten Zellen.</returns>
    public IEnumerable<Cell> GetNeighbors(Cell cell)
    {
        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                var nx = cell.X + dx;
                var ny = cell.Y + dy;

                var neighbor = new Cell(nx, ny);
                if (IsInside(neighbor))
                {
                    yield return neighbor;
                }
            }
        }
    }
}
