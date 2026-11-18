using UnityEngine;

public static class SpriteFactory
{
    static Sprite block;
    static Sprite pixel;

    public static Sprite Block
    {
        get
        {
            if (block == null) block = CreateBlock();
            return block;
        }
    }

    public static Sprite Pixel
    {
        get
        {
            if (pixel == null)
            {
                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                pixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
            }
            return pixel;
        }
    }

    static Sprite CreateBlock()
    {
        const int size = 32;
        const int border = 3;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float shade;
                bool edge = x < border || y < border || x >= size - border || y >= size - border;
                if (edge)
                    shade = (x < border || y >= size - border) ? 1f : 0.55f;
                else
                    shade = 0.85f;

                if (!edge && x < border + 5 && y >= size - border - 5 && x + (size - y) < border * 2 + 8)
                    shade = 1f;

                pixels[y * size + x] = new Color(shade, shade, shade, 1f);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
