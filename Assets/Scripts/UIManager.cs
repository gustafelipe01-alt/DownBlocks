using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    static readonly Color LabelColor = new Color(0.60f, 0.65f, 0.85f);
    static readonly Color ValueColor = Color.white;

    GameManager game;
    Camera cam;
    Font font;

    Text scoreText, highText, levelText, linesText, overlayText, popupText;
    Image overlay;
    RectTransform popupRect;
    Coroutine popupRoutine;
    GameState lastState = (GameState)(-1);

    readonly List<(RectTransform rect, Vector3 world)> anchored = new List<(RectTransform, Vector3)>();

    public void Build(GameManager gm, Camera camera)
    {
        game = gm;
        cam = camera;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var canvasGo = new GameObject("Interface", typeof(RectTransform));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;
        Transform root = canvasGo.transform;

        const float lx = -1.3f;
        AddAnchored(root, "PONTOS", new Vector3(lx, 19.4f), 30, LabelColor, TextAnchor.UpperRight);
        scoreText = AddAnchored(root, "0", new Vector3(lx, 18.6f), 56, ValueColor, TextAnchor.UpperRight);
        AddAnchored(root, "RECORDE", new Vector3(lx, 16.4f), 30, LabelColor, TextAnchor.UpperRight);
        highText = AddAnchored(root, "0", new Vector3(lx, 15.6f), 44, ValueColor, TextAnchor.UpperRight);
        AddAnchored(root, "NÍVEL", new Vector3(lx, 13.4f), 30, LabelColor, TextAnchor.UpperRight);
        levelText = AddAnchored(root, "1", new Vector3(lx, 12.6f), 56, ValueColor, TextAnchor.UpperRight);
        AddAnchored(root, "LINHAS", new Vector3(lx, 10.4f), 30, LabelColor, TextAnchor.UpperRight);
        linesText = AddAnchored(root, "0", new Vector3(lx, 9.6f), 56, ValueColor, TextAnchor.UpperRight);

        AddAnchored(root, "PRÓXIMA", new Vector3(13f, 19.4f), 30, LabelColor, TextAnchor.UpperCenter);
        AddAnchored(root,
            "<b>CONTROLES</b>\n" +
            "Setas esq./dir. ou A / D  ·  mover\n" +
            "Seta p/ cima, W ou X  ·  girar\n" +
            "Z ou Q  ·  girar ao contrário\n" +
            "Seta p/ baixo ou S  ·  descer rápido\n" +
            "Espaço  ·  queda instantânea\n" +
            "P ou Esc  ·  pausar\n" +
            "R  ·  reiniciar",
            new Vector3(10.6f, 12.5f), 26, LabelColor, TextAnchor.UpperLeft);

        popupText = AddAnchored(root, "", new Vector3((Board.Width - 1) * 0.5f, 12f), 60, Color.white, TextAnchor.MiddleCenter);
        popupText.fontStyle = FontStyle.Bold;
        popupRect = popupText.rectTransform;

        var overlayGo = new GameObject("Sobreposicao", typeof(RectTransform));
        overlayGo.transform.SetParent(root, false);
        Stretch(overlayGo.GetComponent<RectTransform>());
        overlay = overlayGo.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.75f);

        overlayText = CreateText(overlayGo.transform, "", 44, Color.white, TextAnchor.MiddleCenter);
        Stretch(overlayText.rectTransform);
    }

    void LateUpdate()
    {
        if (cam == null) return;
        foreach (var (rect, world) in anchored)
            rect.position = cam.WorldToScreenPoint(world);
    }

    public void Refresh()
    {
        scoreText.text = game.Score.ToString();
        highText.text = game.HighScore.ToString();
        levelText.text = game.Level.ToString();
        linesText.text = game.Lines.ToString();

        if (game.State == lastState) return;
        lastState = game.State;

        switch (game.State)
        {
            case GameState.Title:
                overlay.gameObject.SetActive(true);
                overlayText.text =
                    "<size=110><b><color=#4DD9F2>DOWN</color><color=#FFCC40>BLOCKS</color></b></size>\n\n" +
                    "Encaixe as peças e complete linhas para pontuar.\n" +
                    "A cada 10 linhas o nível sobe e as peças caem mais rápido.\n\n" +
                    "<b>Pressione ENTER para começar</b>";
                break;

            case GameState.Paused:
                overlay.gameObject.SetActive(true);
                overlayText.text = "<size=90><b>PAUSADO</b></size>\n\nP ou Esc para continuar\nR para reiniciar";
                break;

            case GameState.GameOver:
                overlay.gameObject.SetActive(true);
                overlayText.text =
                    "<size=90><b><color=#FF6673>FIM DE JOGO</color></b></size>\n\n" +
                    "Pontos: <b>" + game.Score + "</b>\n" +
                    "Linhas: " + game.Lines + "   ·   Nível: " + game.Level + "\n" +
                    (game.NewHighScore ? "<color=#FFCC40><b>NOVO RECORDE!</b></color>\n" : "") +
                    "\n<b>Pressione ENTER para jogar de novo</b>";
                break;

            default:
                overlay.gameObject.SetActive(false);
                break;
        }
    }

    public void ShowPopup(string message)
    {
        if (popupRoutine != null) StopCoroutine(popupRoutine);
        popupRoutine = StartCoroutine(PopupRoutine(message));
    }

    IEnumerator PopupRoutine(string message)
    {
        popupText.text = message;
        const float duration = 1.0f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;
            popupText.color = new Color(1f, 1f, 1f, 1f - k * k);
            popupRect.localScale = Vector3.one * (1f + 0.25f * (1f - k));
            yield return null;
        }
        popupText.text = "";
    }

    Text AddAnchored(Transform parent, string content, Vector3 world, int size, Color color, TextAnchor align)
    {
        Text text = CreateText(parent, content, size, color, align);
        RectTransform rt = text.rectTransform;
        rt.sizeDelta = new Vector2(600, 400);
        rt.pivot = PivotFor(align);
        anchored.Add((rt, world));
        return text;
    }

    Text CreateText(Transform parent, string content, int size, Color color, TextAnchor align)
    {
        var go = new GameObject("Texto", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = align;
        text.supportRichText = true;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static Vector2 PivotFor(TextAnchor a)
    {
        switch (a)
        {
            case TextAnchor.UpperLeft: return new Vector2(0f, 1f);
            case TextAnchor.UpperCenter: return new Vector2(0.5f, 1f);
            case TextAnchor.UpperRight: return new Vector2(1f, 1f);
            case TextAnchor.MiddleLeft: return new Vector2(0f, 0.5f);
            case TextAnchor.MiddleRight: return new Vector2(1f, 0.5f);
            case TextAnchor.LowerLeft: return new Vector2(0f, 0f);
            case TextAnchor.LowerCenter: return new Vector2(0.5f, 0f);
            case TextAnchor.LowerRight: return new Vector2(1f, 0f);
            default: return new Vector2(0.5f, 0.5f);
        }
    }
}
