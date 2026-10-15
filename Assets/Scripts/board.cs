using System.Collections.Generic;
using UnityEngine;

public class board : MonoBehaviour
{
    public const int Size = 8;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private Transform cellTransform;
    [SerializeField] private    GameManager gameManager;

    private readonly Cell[,] cells = new Cell[Size, Size];

    private readonly int[,] data = new int[Size, Size]; // 0 empty, 1 hover, 2 normal

    private readonly List<Vector2Int> hoverPoints = new();

    private readonly List<int> fullLineColumns = new();
    private readonly List<int> fullLineRows = new();

    private readonly List<int> highlightPolyominoColumns = new();
    private readonly List<int> highlightPolyominoRows = new();


    private void Start()
    {
        for (var r = 0; r < Size; r++)
        {
            for (var c = 0; c < Size; c++)
            {
                cells[r, c] = Instantiate(cellPrefab, cellTransform);
                cells[r, c].transform.position = new(c + 0.5f, r + 0.5f, 0.0f);
                cells[r, c].Hide();
            }
        }
    }

    public void Hover(Vector2Int point, int polyominoIndex)
    {
        var polyomino = Polyominos.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var polyominoColumns = polyomino.GetLength(1);

        Unhover();
        Unhighlight();
        HoverPoints(point, polyominoRows, polyominoColumns, polyomino);
        highlightPolyominoColumns.Clear();
        highlightPolyominoRows.Clear();
        if (hoverPoints.Count > 0)
        {
            Hover();
            Highlight(point, polyominoColumns, polyominoRows);

            foreach (var fullLineColumn in fullLineColumns)
            {
                highlightPolyominoColumns.Add(fullLineColumn - point.x);
            }
            foreach (var fullLineRow in fullLineRows)
            {
             
                highlightPolyominoRows.Add(fullLineRow - point.y);
            }
        }
    }

    private void Unhighlight()
    {
        UnHighlightFullLineColumns();
        UnHighlightFullLineRows();
        fullLineColumns.Clear();
        fullLineRows.Clear();
    }

    private void Highlight(Vector2Int point, int polyominoColumns, int polyominoRows)
    {
        PredictFullLineColumns(point.x, point.x + polyominoColumns);
        PredictFullLineRows(point.y, point.y + polyominoRows);

        HighlightFullLineColumns();
        HighlightFullLineRows();
    }

    private void HighlightFullLineRows()
    {
        foreach (var r in fullLineRows)
        {
            for (var c = 0; c < Size; c++)
            {
                if (data[r, c] == 2)
                {
                    cells[r, c].Highlight();
                }
            }
        }
    }

    private void HighlightFullLineColumns()
    {
        foreach (var c in fullLineColumns)
        {
            for (var r = 0; r < Size; r++)
            {
                if (data[r, c] == 2)
                {
                    cells[r, c].Highlight();
                }
            }
        }
    }

    private void UnHighlightFullLineRows()
    {
        foreach (var r in fullLineRows)
        {
            for (var c = 0; c < Size; c++)
            {
                if (data[r, c] == 2)
                {
                    cells[r, c].Normal();
                }
            }
        }
    }

    private void UnHighlightFullLineColumns()
    {
        foreach (var c in fullLineColumns)
        {
            for (var r = 0; r < Size; r++)
            {
                if (data[r, c] == 2)
                {
                    cells[r, c].Normal();
                }
            }
        }
    }

    private void PredictFullLineRows(int fromRow, int toRowExclusive)
    {
        fullLineRows.Clear();

        for (var r = Mathf.Max(0, fromRow); r < Mathf.Min(Size, toRowExclusive); r++)
        {
            var isFullLine = true;
            for (var c = 0; c < Size; c++)
            {
                if (data[r, c] != 1 && data[r, c] != 2)
                {
                    isFullLine = false;
                    break;
                }
            }
            if (isFullLine == true)
            {
                fullLineRows.Add(r);
            }
        }
    }

    private void PredictFullLineColumns(int fromColumn, int toColumnExclusive)
    {
        fullLineColumns.Clear();
        for (var c = Mathf.Max(0, fromColumn); c < Mathf.Min(Size, toColumnExclusive); c++)
        {
            var isFullLine = true;
            for (var r = 0; r < Size; r++)
            {
                if (data[r, c] != 1 && data[r, c] != 2)
                {
                    isFullLine = false;
                    break;
                }
            }
            if (isFullLine == true)
            {
                fullLineColumns.Add(c);
            }
        }
    }

    private void HoverPoints(Vector2Int point, int polyominoRows, int polyominoColumns, int[,] polyomino)
    {
        for (var r = 0; r < polyominoRows; r++)
        {
            for (var c = 0; c < polyominoColumns; c++)
            {
                if (polyomino[r, c] > 0)
                {
                    var hoverPoint = point + new Vector2Int(c, r);
                    if (IsValidPoint(hoverPoint) == false)
                    {
                        hoverPoints.Clear();
                        return;
                    }

                    hoverPoints.Add(hoverPoint);
                }
            }
        }
    }

    private bool IsValidPoint(Vector2Int point)
    {
        if (point.x < 0 || Size <= point.x) return false;
        if (point.y < 0 || Size <= point.y) return false;
        if (data[point.y, point.x] > 0) return false;

        return true;
    }

    private void Hover()
    {

        foreach (var p in hoverPoints)
        {
            data[p.y, p.x] = 1;
            cells[p.y, p.x].Hover();
        }
    }

    private void Unhover()
    {
        foreach (var p in hoverPoints)
        {
            data[p.y, p.x] = 0;
            cells[p.y, p.x].Hide();
        }
        hoverPoints.Clear();
    }

    public bool Place(Vector2Int point, int polyominoIndex)
    {
        var polyomino = Polyominos.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var polyominoColumns = polyomino.GetLength(1);

        Unhover();
        Unhighlight();
        HoverPoints(point, polyominoRows, polyominoColumns, polyomino);
        if (hoverPoints.Count > 0)
        {
            Place(point, polyominoColumns, polyominoRows);
            return true;
        }
        return false;
    }

    private void Place(Vector2Int point, int polyominoColumns, int polyominoRows)
    {
        var placedCells = hoverPoints.Count;
        foreach (var p in hoverPoints)
        {
            data[p.y, p.x] = 2;
            cells[p.y, p.x].Normal();
        }
        ClearFullLines(point, polyominoColumns, polyominoRows);
        hoverPoints.Clear();

        var clearedLines = fullLineColumns.Count + fullLineRows.Count;
        if (clearedLines > 0)
        {
            SFXManager.PlayStoneBreak(); 
        }
        gameManager.AddScore(placedCells, clearedLines);
    }

    public void ResetBoard()
    {
        hoverPoints.Clear();
        fullLineColumns.Clear();
        fullLineRows.Clear();
        highlightPolyominoColumns.Clear();
        highlightPolyominoRows.Clear();

        for (var r = 0; r < Size; r++)
        {
            for (var c = 0; c < Size; c++)
            {
                data[r, c] = 0;
                cells[r, c].Hide();
            }
        }
    }

    private void ClearFullLines(Vector2Int point, int polyominoColumns, int polyominoRows)
    {
        FullLineColumns(point.x, point.x + polyominoColumns);
        FullLineRows(point.y, point.y + polyominoRows);

        ClearFullLineColumns();
        ClearFullLineRows();
    }

    private void FullLineColumns(int fromColumn, int toColumnExclusive)
    {
        fullLineColumns.Clear();
        for (var c = Mathf.Max(0, fromColumn); c < Mathf.Min(Size, toColumnExclusive); c++)
        {
            var isFullLine = true;
            for (var r = 0; r < Size; r++)
            {
                if (data[r, c] != 2)
                {
                    isFullLine = false;
                    break;
                }
            }
            if (isFullLine == true)
            {
                fullLineColumns.Add(c);
            }
        }
    }

    private void FullLineRows(int fromRow, int toRowExclusive)
    {
        fullLineRows.Clear();
        for (var r = Mathf.Max(0, fromRow); r < Mathf.Min(Size, toRowExclusive); r++)
        {
            var isFullLine = true;
            for (var c = 0; c < Size; c++)
            {
                if (data[r, c] != 2)
                {
                    isFullLine = false;
                    break;
                }
            }
            if (isFullLine == true)
            {
                fullLineRows.Add(r);
            }
        }
    }

    private void ClearFullLineColumns()
    {
        foreach (var c in fullLineColumns)
        {
            for (var r = 0; r < Size; r++)
            {
                data[r, c] = 0;
                cells[r, c].Hide();
            }
        }
    }

    private void ClearFullLineRows()
    {
        foreach (var r in fullLineRows)
        {
            for (var c = 0; c < Size; c++)
            {
                data[r, c] = 0;
                cells[r, c].Hide();
            }
        }
    }

    public bool CheckPlace(int polyominoIndex)
    {
        var polyomino = Polyominos.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var polyominoColumns = polyomino.GetLength(1);


        for (var r = 0; r <= Size - polyominoRows; r++)
        {
            for (var c = 0; c <= Size - polyominoColumns; c++)
            {
                if (CheckPlace(c, r, polyominoColumns, polyominoRows, polyomino) == true)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckPlace(int column, int row, int polyominoColumns, int polyominoRows, int[,] polyomino)
    {
        for (var r = 0; r < polyominoRows; r++)
        {
            for (var c = 0; c < polyominoColumns; c++)
            {
                if (polyomino[r, c] > 0 && data[row + r, column + c] > 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public List<int> HighlightPolyominoColumns => highlightPolyominoColumns;
    public List<int> HighlightPolyominoRows => highlightPolyominoRows;
}