using UnityEngine;

public class Block : MonoBehaviour
{
    public const int Size = 5;

    [SerializeField] private Cell cellPrefab;

    private readonly Vector3 inputOffset =  new (0f,2f,0f);
    private readonly Cell[,] cells = new Cell[Size, Size];

    private Vector3 previousMousePosition = Vector3.positiveInfinity;

    private Vector3 position;
    private Vector3 scale;
    private Vector2 inputPoint;
    private Camera mainCamera;
    private Vector2Int previousDragPoint;
    private Vector2Int currentDragPoint;
    private Vector2 center;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Initialize()
    {
        for(var r = 0; r < Size; r++)
        {
            for(var c = 0;c < Size; c++)
            {
                cells[r, c] = Instantiate(cellPrefab, transform);
            }
        }
        position = transform.localPosition;
        scale = transform.localScale;
    }

    public void Show( int polyominosIndex)
    {
        Hide();

        var polyomino = Polyominos.Get(polyominosIndex);
        var polyominoRows = polyomino.GetLength(0);
        var polyominoColumns = polyomino.GetLength(1);
        var center = new Vector2(polyominoColumns * 0.5f, polyominoRows * 0.5f);
        for(var r =0;r < polyominoRows; r++)
        {
            for(var c = 0;c < polyominoColumns; c++)
            {
                if (polyomino[r,c] > 0)
                {
                    cells[r, c].transform.localPosition = new(c - center.x + .5f, r - center.y + .5f, 0.0f);
                    cells[r, c].Normal();
                }
            }
        }
    }
    private void Hide()
    {
        for (var r = 0; r < Size; r++)
        {
            for (var c = 0; c < Size; c++)
            {
                cells[r, c].Hide();
            }
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("OnMouseDown");
        inputPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.localPosition = position + inputOffset;
        transform.localScale = Vector3.one;
        currentDragPoint = Vector2Int.RoundToInt((Vector2)transform.position - center);
        Debug.Log($"Drag point {currentDragPoint}");
        previousDragPoint = currentDragPoint;

        previousMousePosition = Input.mousePosition;
    }
    private void OnMouseUp()
    {
        transform.localPosition = position;
        transform.localScale = scale;
        Debug.Log("OnMouseUp");
        previousMousePosition = Vector3.positiveInfinity;
    }
    private void OnMouseDrag()
    {
        var currentMousePosition = Input.mousePosition;
        if (currentMousePosition != previousMousePosition)
        {
            previousMousePosition = currentMousePosition;
            Debug.Log("OnMouseDrag");

            var inputDelta = (Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition) - inputPoint;
            transform.position = position + inputOffset + (Vector3)inputDelta*1.4f;

            currentDragPoint = Vector2Int.RoundToInt((Vector2)transform.position - center);
            if(currentDragPoint != previousDragPoint)
            {
                previousDragPoint = currentDragPoint;
                Debug.Log($"Drag point {currentDragPoint}");
            }
        }
    }
}
