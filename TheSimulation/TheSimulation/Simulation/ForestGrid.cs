using System.Runtime.CompilerServices;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInside(Cell c)
    {
        // Casting to uint handles the negative check (c.X >= 0)
        // and the upper bound check (c.X < Cols) in one operation.
        // This works because casting a negative integer to uint results in a very large positive number,
        // which will always be greater than or equal to Cols or Rows.
        // This optimization reduces the number of comparisons needed from four to two.
        var withinHorizontalBounds = IsInUpperLimit(c.X, Cols);
        var withinVerticalBounds = IsInUpperLimit(c.Y, Rows);

        return withinHorizontalBounds && withinVerticalBounds;

        // Local function for clarification
        static bool IsInUpperLimit(int value, int limit) => (uint)value < (uint)limit;
    }

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
    /// Allocation-free mit festen Offsets und Span.
    /// </summary>
    /// <param name="cell">Die Zelle, deren Nachbarn ermittelt werden sollen.</param>
    /// <param name="neighbors">Ein Span, in den die Nachbarn geschrieben werden.</param>
    /// <returns>Anzahl der gültigen Nachbarn.</returns>
    public int GetNeighbors(Cell cell, Span<Cell> neighbors)
    {
        var count = 0;

        foreach (var (dx, dy) in NeighborOffsets)
        {
            var nx = cell.X + dx;
            var ny = cell.Y + dy;
            var neighbor = new Cell(nx, ny);

            if (IsInside(neighbor))
            {
                neighbors[count] = neighbor;
                count++;
            }
        }

        return count;
    }

    private static readonly (int dx, int dy)[] NeighborOffsets =
    [
        (-1, -1), (0, -1), (1, -1),
        (-1, 0),           (1, 0),
        (-1, 1),  (0, 1),  (1, 1)
    ];
}
