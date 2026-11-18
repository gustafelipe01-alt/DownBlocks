using UnityEngine;

public enum ShapeType { I, O, T, S, Z, J, L }

public static class BlockShapes
{
    public static readonly Vector2Int[][] Cells =
    {
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) },
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
        new[] { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0) },
        new[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) },
        new[] { new Vector2Int(1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) },
    };

    public static readonly Color[] Colors =
    {
        new Color(0.30f, 0.85f, 0.95f),
        new Color(1.00f, 0.80f, 0.25f),
        new Color(0.70f, 0.45f, 1.00f),
        new Color(0.45f, 0.90f, 0.45f),
        new Color(1.00f, 0.40f, 0.45f),
        new Color(0.35f, 0.55f, 1.00f),
        new Color(1.00f, 0.60f, 0.25f),
    };

    public static Vector2 Pivot(ShapeType type)
    {
        return (type == ShapeType.I || type == ShapeType.O) ? new Vector2(0.5f, 0.5f) : Vector2.zero;
    }
}
