using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int Width = 10;
    public const int Height = 20;
    const int BufferRows = 4;
    const int GridHeight = Height + BufferRows;

    readonly Transform[,] grid = new Transform[Width, GridHeight];

    void Awake()
    {
        BuildBackground();
    }

    public bool IsFree(Vector2Int p)
    {
        if (p.x < 0 || p.x >= Width || p.y < 0 || p.y >= GridHeight) return false;
        return grid[p.x, p.y] == null;
    }

    public bool Lock(Piece piece)
    {
        bool insideVisibleArea = true;
        Vector2Int pos = piece.GridPos;

        for (int i = 0; i < piece.Blocks.Length; i++)
        {
            Vector2Int cell = pos + piece.Cells[i];
            Transform block = piece.Blocks[i];
            block.SetParent(transform, true);
            block.position = new Vector3(cell.x, cell.y, 0f);
            block.GetComponent<SpriteRenderer>().sortingOrder = 1;
            grid[cell.x, cell.y] = block;

            if (cell.y >= Height) insideVisibleArea = false;
        }

        piece.ReleaseBlocks();
        return insideVisibleArea;
    }

    public List<int> FindFullRows()
    {
        var rows = new List<int>();
        for (int y = 0; y < GridHeight; y++)
        {
            bool full = true;
            for (int x = 0; x < Width; x++)
            {
                if (grid[x, y] == null) { full = false; break; }
            }
            if (full) rows.Add(y);
        }
        return rows;
    }

    public IEnumerator ClearRows(List<int> rows)
    {
        foreach (int y in rows)
            for (int x = 0; x < Width; x++)
                grid[x, y].GetComponent<SpriteRenderer>().color = Color.white;

        yield return new WaitForSeconds(0.18f);

        rows.Sort();
        for (int i = rows.Count - 1; i >= 0; i--)
        {
            int row = rows[i];
            for (int x = 0; x < Width; x++)
            {
                Destroy(grid[x, row].gameObject);
                grid[x, row] = null;
            }

            for (int y = row + 1; y < GridHeight; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Transform block = grid[x, y];
                    grid[x, y - 1] = block;
                    grid[x, y] = null;
                    if (block != null) block.position += Vector3.down;
                }
            }
        }
    }

    public void Clear()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (grid[x, y] != null) Destroy(grid[x, y].gameObject);
                grid[x, y] = null;
            }
        }
    }

    void BuildBackground()
    {
        var center = new Vector3((Width - 1) * 0.5f, (Height - 1) * 0.5f, 0f);

        CreateRect("Fundo", center, new Vector2(Width, Height), new Color(0.09f, 0.10f, 0.17f), -10);

        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                CreateRect("Celula", new Vector3(x, y, 0f), new Vector2(0.92f, 0.92f), new Color(0.12f, 0.14f, 0.23f), -9);

        const float t = 0.2f;
        var frame = new Color(0.55f, 0.60f, 0.85f);
        float left = -0.5f - t * 0.5f, right = Width - 0.5f + t * 0.5f;
        float bottom = -0.5f - t * 0.5f, top = Height - 0.5f + t * 0.5f;
        CreateRect("Moldura", new Vector3(left, center.y, 0f), new Vector2(t, Height + t * 2), frame, 20);
        CreateRect("Moldura", new Vector3(right, center.y, 0f), new Vector2(t, Height + t * 2), frame, 20);
        CreateRect("Moldura", new Vector3(center.x, bottom, 0f), new Vector2(Width + t * 2, t), frame, 20);
        CreateRect("Moldura", new Vector3(center.x, top, 0f), new Vector2(Width + t * 2, t), frame, 20);
    }

    public static GameObject CreateRect(string name, Vector3 position, Vector2 size, Color color, int sortingOrder, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Pixel;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return go;
    }

    GameObject CreateRect(string name, Vector3 position, Vector2 size, Color color, int sortingOrder)
    {
        return CreateRect(name, position, size, color, sortingOrder, transform);
    }
}
