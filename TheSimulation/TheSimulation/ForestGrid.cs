namespace TheSimulation;

public sealed class ForestGrid(int cols, int rows)
{
    public int Cols { get; } = cols;
    public int Rows { get; } = rows;

    private readonly ForestCellState[,] cells = new ForestCellState[cols, rows];

    public ForestCellState this[Cell c]
    {
        get => cells[c.X, c.Y];
        set => cells[c.X, c.Y] = value;
    }

    public void SetTree(Cell c) => this[c] = ForestCellState.Tree;
    public void SetBurning(Cell c) => this[c] = ForestCellState.Burning;
    public void Clear(Cell c) => this[c] = ForestCellState.Empty;

    public bool IsInside(Cell c)
        => c.X >= 0 && c.Y >= 0 && c.X < Cols && c.Y < Rows;

    public bool IsTree(Cell c)
    => IsInside(c) && this[c] == ForestCellState.Tree;

    public bool IsBurning(Cell c)
        => IsInside(c) && this[c] == ForestCellState.Burning;

    public bool IsEmpty(Cell c)
        => IsInside(c) && this[c] == ForestCellState.Empty;

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
