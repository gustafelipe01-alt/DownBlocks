using UnityEngine;

public class Piece : MonoBehaviour
{
    static readonly Vector2Int[] Kicks =
    {
        new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(0, 1), new Vector2Int(2, 0), new Vector2Int(-2, 0)
    };

    public ShapeType Type { get; private set; }
    public Vector2Int[] Cells { get; private set; } = new Vector2Int[0];
    public Transform[] Blocks { get; private set; } = new Transform[0];

    public Vector2Int GridPos
    {
        get => new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        set => transform.position = new Vector3(value.x, value.y, 0f);
    }

    public void SetShape(ShapeType type, float alpha, int sortingOrder)
    {
        DestroyBlocks();

        Type = type;
        Cells = (Vector2Int[])BlockShapes.Cells[(int)type].Clone();
        Blocks = new Transform[Cells.Length];

        Color color = BlockShapes.Colors[(int)type];
        color.a = alpha;

        for (int i = 0; i < Cells.Length; i++)
        {
            var go = new GameObject("Bloco");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Block;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            Blocks[i] = go.transform;
        }

        UpdateBlockPositions();
    }

    public void CopyFrom(Piece other, float alpha, int sortingOrder)
    {
        SetShape(other.Type, alpha, sortingOrder);
        Cells = (Vector2Int[])other.Cells.Clone();
        UpdateBlockPositions();
    }

    public void ReleaseBlocks()
    {
        Blocks = new Transform[0];
        Cells = new Vector2Int[0];
    }

    public void DestroyBlocks()
    {
        foreach (var b in Blocks)
            if (b != null) Destroy(b.gameObject);
        ReleaseBlocks();
    }

    public bool Fits(Board board, Vector2Int pos, Vector2Int[] cells)
    {
        foreach (var c in cells)
            if (!board.IsFree(pos + c)) return false;
        return true;
    }

    public bool Fits(Board board) => Fits(board, GridPos, Cells);

    public bool TryMove(Board board, Vector2Int delta)
    {
        Vector2Int target = GridPos + delta;
        if (!Fits(board, target, Cells)) return false;
        GridPos = target;
        return true;
    }

    public bool TryRotate(Board board, bool clockwise)
    {
        if (Type == ShapeType.O) return false;

        Vector2Int[] rotated = RotatedCells(clockwise);
        foreach (var kick in Kicks)
        {
            if (Fits(board, GridPos + kick, rotated))
            {
                Cells = rotated;
                GridPos += kick;
                UpdateBlockPositions();
                return true;
            }
        }
        return false;
    }

    Vector2Int[] RotatedCells(bool clockwise)
    {
        Vector2 pivot = BlockShapes.Pivot(Type);
        var result = new Vector2Int[Cells.Length];
        for (int i = 0; i < Cells.Length; i++)
        {
            float x = Cells[i].x - pivot.x;
            float y = Cells[i].y - pivot.y;
            float nx = clockwise ? y : -y;
            float ny = clockwise ? -x : x;
            result[i] = new Vector2Int(Mathf.RoundToInt(nx + pivot.x), Mathf.RoundToInt(ny + pivot.y));
        }
        return result;
    }

    public Vector2 CellsCenter()
    {
        if (Cells.Length == 0) return Vector2.zero;
        Vector2 min = Cells[0], max = Cells[0];
        foreach (var c in Cells)
        {
            min = Vector2.Min(min, c);
            max = Vector2.Max(max, c);
        }
        return (min + max) * 0.5f;
    }

    void UpdateBlockPositions()
    {
        for (int i = 0; i < Blocks.Length; i++)
            Blocks[i].localPosition = new Vector3(Cells[i].x, Cells[i].y, 0f);
    }
}
