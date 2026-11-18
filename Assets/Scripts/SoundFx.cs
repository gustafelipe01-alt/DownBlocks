using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundFx : MonoBehaviour
{
    const int SampleRate = 44100;

    AudioSource source;
    AudioClip move, rotate, drop, lockClip, clear, bigClear, gameOver, start;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;

        move     = Tone("mover",     220f, 220f, 0.03f, 0.10f, true);
        rotate   = Tone("girar",     440f, 560f, 0.05f, 0.10f, true);
        drop     = Tone("queda",     260f,  70f, 0.10f, 0.35f, false);
        lockClip = Tone("encaixe",   150f,  90f, 0.07f, 0.30f, false);
        clear    = Tone("linha",     523f, 1046f, 0.25f, 0.15f, true);
        bigClear = Tone("quatro",    392f, 1568f, 0.45f, 0.18f, true);
        gameOver = Tone("fim",       330f,  60f, 0.80f, 0.18f, true);
        start    = Tone("comecar",   660f, 990f, 0.15f, 0.12f, true);
    }

    public void Move() => source.PlayOneShot(move);
    public void Rotate() => source.PlayOneShot(rotate);
    public void Drop() => source.PlayOneShot(drop);
    public void Lock() => source.PlayOneShot(lockClip);
    public void Clear(int lines) => source.PlayOneShot(lines >= 4 ? bigClear : clear);
    public void GameOver() => source.PlayOneShot(gameOver);
    public void NewGame() => source.PlayOneShot(start);

    static AudioClip Tone(string name, float freqStart, float freqEnd, float duration, float volume, bool square)
    {
        int length = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        var data = new float[length];
        float phase = 0f;

        for (int i = 0; i < length; i++)
        {
            float t = (float)i / length;
            float freq = Mathf.Lerp(freqStart, freqEnd, t);
            phase += 2f * Mathf.PI * freq / SampleRate;
            float wave = Mathf.Sin(phase);
            if (square) wave = wave >= 0f ? 1f : -1f;
            data[i] = wave * volume * (1f - t);
        }

        var clip = AudioClip.Create(name, length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
