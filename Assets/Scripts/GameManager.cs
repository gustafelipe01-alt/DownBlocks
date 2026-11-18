using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { Title, Playing, ClearingLines, Paused, GameOver }

[RequireComponent(typeof(SoundFx))]
public class GameManager : MonoBehaviour
{
    static readonly int[] LinePoints = { 0, 100, 300, 500, 800 };
    const int LinesPerLevel = 10;
    const float DasDelay = 0.17f;
    const float DasRepeat = 0.05f;
    const float SoftDropInterval = 0.04f;
    const string HighScoreKey = "DownBlocks_Recorde";

    static readonly Vector2Int SpawnPos = new Vector2Int(4, Board.Height - 2);
    static readonly Vector2 NextPreviewCenter = new Vector2(13f, 16.3f);

    public GameState State { get; private set; } = GameState.Title;
    public int Score { get; private set; }
    public int Lines { get; private set; }
    public int Level { get; private set; } = 1;
    public int HighScore { get; private set; }
    public bool NewHighScore { get; private set; }

    Board board;
    Piece current, ghost, next;
    ShapeType nextType;
    UIManager ui;
    SoundFx sfx;
    Camera cam;
    Vector3 camBasePos;

    readonly List<ShapeType> bag = new List<ShapeType>();
    float fallTimer;
    float dasTimer;
    int dasDir;

    float FallInterval => Mathf.Max(0.07f, 0.8f * Mathf.Pow(0.85f, Level - 1));

    void Awake()
    {
        sfx = GetComponent<SoundFx>();
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        SetupCamera();

        board = new GameObject("Tabuleiro").AddComponent<Board>();
        current = new GameObject("PecaAtual").AddComponent<Piece>();
        ghost = new GameObject("PecaFantasma").AddComponent<Piece>();
        next = new GameObject("ProximaPeca").AddComponent<Piece>();

        Board.CreateRect("CaixaProxima", NextPreviewCenter, new Vector2(5.2f, 3.8f), new Color(0.55f, 0.60f, 0.85f), -11);
        Board.CreateRect("CaixaProximaFundo", NextPreviewCenter, new Vector2(4.8f, 3.4f), new Color(0.09f, 0.10f, 0.17f), -10);

        ui = gameObject.AddComponent<UIManager>();
        ui.Build(this, cam);
    }

    void SetupCamera()
    {
        cam = Camera.main;
        if (cam == null)
        {
            cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.tag = "MainCamera";
        }
        cam.orthographic = true;
        cam.orthographicSize = 11f;
        cam.transform.position = new Vector3((Board.Width - 1) * 0.5f, (Board.Height - 1) * 0.5f, -10f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.09f);
        camBasePos = cam.transform.position;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        switch (State)
        {
            case GameState.Title:
            case GameState.GameOver:
                if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                    StartGame();
                break;

            case GameState.Paused:
                if (kb.pKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)
                    State = GameState.Playing;
                else if (kb.rKey.wasPressedThisFrame)
                    StartGame();
                break;

            case GameState.Playing:
                HandlePlaying(kb);
                break;

            case GameState.ClearingLines:
                break;
        }

        ghost.gameObject.SetActive(State == GameState.Playing || State == GameState.Paused);
        ui.Refresh();
    }

    void HandlePlaying(Keyboard kb)
    {
        float dt = Time.deltaTime;

        if (kb.pKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame) { State = GameState.Paused; return; }
        if (kb.rKey.wasPressedThisFrame) { StartGame(); return; }

        bool leftHeld = kb.leftArrowKey.isPressed || kb.aKey.isPressed;
        bool rightHeld = kb.rightArrowKey.isPressed || kb.dKey.isPressed;
        bool leftPressed = kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame;
        bool rightPressed = kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;

        if (leftPressed) { Shift(-1); dasDir = -1; dasTimer = DasDelay; }
        else if (rightPressed) { Shift(1); dasDir = 1; dasTimer = DasDelay; }
        else if (dasDir != 0)
        {
            bool held = dasDir < 0 ? leftHeld : rightHeld;
            if (!held) dasDir = 0;
            else
            {
                dasTimer -= dt;
                if (dasTimer <= 0f) { Shift(dasDir); dasTimer = DasRepeat; }
            }
        }

        if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.xKey.wasPressedThisFrame)
            Rotate(true);
        if (kb.zKey.wasPressedThisFrame || kb.qKey.wasPressedThisFrame)
            Rotate(false);

        if (kb.spaceKey.wasPressedThisFrame) { HardDrop(); return; }

        bool softDrop = kb.downArrowKey.isPressed || kb.sKey.isPressed;
        float interval = softDrop ? Mathf.Min(SoftDropInterval, FallInterval) : FallInterval;

        fallTimer += dt;
        if (fallTimer >= interval)
        {
            fallTimer = 0f;
            if (current.TryMove(board, Vector2Int.down))
            {
                if (softDrop) Score += 1;
            }
            else
            {
                LockPiece();
                return;
            }
        }

        UpdateGhost();
    }

    void Shift(int dir)
    {
        if (current.TryMove(board, new Vector2Int(dir, 0))) sfx.Move();
    }

    void Rotate(bool clockwise)
    {
        if (current.TryRotate(board, clockwise))
        {
            sfx.Rotate();
            ghost.CopyFrom(current, 0.25f, 5);
        }
    }

    void HardDrop()
    {
        int cells = 0;
        while (current.TryMove(board, Vector2Int.down)) cells++;
        Score += cells * 2;
        sfx.Drop();
        StartCoroutine(Shake(0.08f, 0.12f));
        LockPiece();
    }

    void LockPiece()
    {
        bool ok = board.Lock(current);
        sfx.Lock();

        if (!ok) { EndGame(); return; }

        List<int> rows = board.FindFullRows();
        if (rows.Count > 0) StartCoroutine(ClearLinesRoutine(rows));
        else SpawnNext();
    }

    IEnumerator ClearLinesRoutine(List<int> rows)
    {
        State = GameState.ClearingLines;
        int count = rows.Count;

        sfx.Clear(count);
        StartCoroutine(Shake(0.15f + 0.05f * count, 0.08f * count));

        yield return board.ClearRows(rows);

        int gained = LinePoints[Mathf.Min(count, 4)] * Level;
        Score += gained;
        Lines += count;

        int oldLevel = Level;
        Level = 1 + Lines / LinesPerLevel;

        string[] names = { "", "LINHA!", "DUPLA!", "TRIPLA!", "QUÁDRUPLA!" };
        string msg = names[Mathf.Min(count, 4)] + "\n+" + gained;
        if (Level > oldLevel) msg += "\nNÍVEL " + Level + "!";
        ui.ShowPopup(msg);

        State = GameState.Playing;
        SpawnNext();
    }

    void StartGame()
    {
        StopAllCoroutines();
        cam.transform.position = camBasePos;

        board.Clear();
        Score = 0;
        Lines = 0;
        Level = 1;
        NewHighScore = false;
        bag.Clear();
        dasDir = 0;

        nextType = DrawFromBag();
        State = GameState.Playing;
        sfx.NewGame();
        SpawnNext();
    }

    void SpawnNext()
    {
        current.SetShape(nextType, 1f, 10);
        current.GridPos = SpawnPos;

        nextType = DrawFromBag();
        next.SetShape(nextType, 1f, 10);
        Vector2 offset = next.CellsCenter();
        next.transform.position = new Vector3(NextPreviewCenter.x - offset.x, NextPreviewCenter.y - offset.y, 0f);

        ghost.CopyFrom(current, 0.25f, 5);
        UpdateGhost();
        fallTimer = 0f;

        if (!current.Fits(board)) EndGame();
    }

    void EndGame()
    {
        State = GameState.GameOver;
        sfx.GameOver();
        StartCoroutine(Shake(0.4f, 0.25f));

        if (Score > HighScore)
        {
            HighScore = Score;
            NewHighScore = true;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
    }

    void UpdateGhost()
    {
        Vector2Int pos = current.GridPos;
        while (current.Fits(board, pos + Vector2Int.down, current.Cells)) pos += Vector2Int.down;
        ghost.GridPos = pos;
    }

    ShapeType DrawFromBag()
    {
        if (bag.Count == 0)
        {
            for (int i = 0; i < 7; i++) bag.Add((ShapeType)i);
            for (int i = bag.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (bag[i], bag[j]) = (bag[j], bag[i]);
            }
        }
        ShapeType t = bag[0];
        bag.RemoveAt(0);
        return t;
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float strength = magnitude * (1f - t / duration);
            cam.transform.position = camBasePos + (Vector3)(Random.insideUnitCircle * strength);
            yield return null;
        }
        cam.transform.position = camBasePos;
    }
}
