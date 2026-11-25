using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Look of the bootstrap screens (server picker, update, loading) shown before the game's
/// own UI exists: a dark blue backdrop with the game's icon and name, rounded panels and
/// gold primary buttons. IMGUI on a canvas at least 1920x1080 scaled to the screen, so
/// controls stay touch-sized. Textures are generated once; no game content is needed.
/// </summary>
public static class SprkUi
{
    public static readonly Color Gold = new Color32(0xE8, 0xB6, 0x4C, 0xFF);
    public static readonly Color Text = new Color32(0xF2, 0xF4, 0xF8, 0xFF);
    public static readonly Color Muted = new Color32(0x9A, 0xA6, 0xBC, 0xFF);
    public static readonly Color ErrorColor = new Color32(0xFF, 0x8A, 0x75, 0xFF);
    static readonly Color Dark = new Color32(0x14, 0x10, 0x08, 0xFF);
    static readonly Color PanelFill = new Color32(0x0F, 0x17, 0x29, 0xF2);
    static readonly Color Edge = new Color32(0x2E, 0x3D, 0x5C, 0xFF);
    static readonly Color RowFill = new Color32(0x17, 0x22, 0x39, 0xFF);
    static readonly Color RowPressed = new Color32(0x22, 0x31, 0x51, 0xFF);
    static readonly Color FieldFill = new Color32(0x09, 0x0F, 0x1C, 0xFF);

    /// <summary>The game's icon (SprkBootstrap's serialized field, set in the generated boot scene).</summary>
    public static Texture2D Logo;

    static float scale = 1f;
    static Texture2D backdrop, glow, dot;
    static readonly Dictionary<string, GUIStyle> cache = new Dictionary<string, GUIStyle>();

    /// <summary>Canvas size: 1920x1080 or larger along the screen's longer side.</summary>
    public static float Width => Screen.width / scale;
    public static float Height => Screen.height / scale;

    /// <summary>Starts drawing on the scaled canvas; pair with <see cref="End"/>.</summary>
    public static void Begin()
    {
        scale = Mathf.Max(0.1f, Mathf.Min(Screen.width / 1920f, Screen.height / 1080f));
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
    }

    public static void End() => GUI.matrix = Matrix4x4.identity;

    /// <summary>Full-screen backdrop with the game's icon and name at the top left.</summary>
    public static void Backdrop()
    {
        if (backdrop == null) backdrop = VerticalGradient(new Color32(0x1C, 0x2B, 0x4D, 0xFF), new Color32(0x04, 0x07, 0x10, 0xFF));
        if (glow == null) glow = RadialGlow();
        GUI.DrawTexture(new Rect(0, 0, Width, Height), backdrop, ScaleMode.StretchToFill);
        var tint = GUI.color;
        GUI.color = new Color(Gold.r, Gold.g, Gold.b, 0.10f);
        GUI.DrawTexture(new Rect(Width / 2f - 900f, -520f, 1800f, 1100f), glow, ScaleMode.StretchToFill);
        GUI.color = tint;

        float x = 56f;
        if (Logo != null)
        {
            GUI.DrawTexture(new Rect(x, 44f, 104f, 104f), Logo, ScaleMode.ScaleToFit);
            x += 128f;
        }
        GUI.Label(new Rect(x, 52f, 900f, 60f), "King's Raid", Style("brand"));
        GUI.Label(new Rect(x + 2f, 110f, 900f, 40f), "SPRK", Style("brandSub"));
    }

    /// <summary>A centered panel of the given size; returns its inner content area.</summary>
    public static Rect Panel(float width, float height, float top = -1f)
    {
        float y = top >= 0 ? top : Mathf.Max(170f, (Height - height) / 2f + 40f);
        var outer = new Rect((Width - width) / 2f, y, width, height);
        GUI.Box(outer, GUIContent.none, Style("panel"));
        return new Rect(outer.x + 56f, outer.y + 44f, outer.width - 112f, outer.height - 88f);
    }

    public static void ProgressBar(Rect r, float progress)
    {
        GUI.Box(r, GUIContent.none, Style("track"));
        Rect fill;
        if (progress >= 0f)
        {
            fill = new Rect(r.x, r.y, Mathf.Max(r.height, r.width * Mathf.Clamp01(progress)), r.height);
            if (progress <= 0f) return;
        }
        else
        {
            // Unknown length: a segment sliding back and forth.
            float t = Mathf.PingPong(Time.realtimeSinceStartup * 0.6f, 1f);
            float w = r.width * 0.25f;
            fill = new Rect(r.x + (r.width - w) * Mathf.SmoothStep(0f, 1f, t), r.y, w, r.height);
        }
        GUI.Box(fill, GUIContent.none, Style("fill"));
    }

    /// <summary>A filled circle (radio marks); 9-sliced boxes distort at this size.</summary>
    public static void Dot(Rect r, Color color)
    {
        if (dot == null) dot = Rounded(Color.white, Color.white, 16, 0, 34);
        var tint = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(r, dot, ScaleMode.StretchToFill);
        GUI.color = tint;
    }

    /// <summary>Radio mark: a ring, filled gold when selected.</summary>
    public static void Radio(Rect r, bool selected)
    {
        Dot(r, selected ? Gold : Muted);
        Dot(new Rect(r.x + 3f, r.y + 3f, r.width - 6f, r.height - 6f), new Color32(0x0B, 0x11, 0x1F, 0xFF));
        if (selected) Dot(new Rect(r.x + 8f, r.y + 8f, r.width - 16f, r.height - 16f), Gold);
    }

    /// <summary>Twelve dots fading around a circle.</summary>
    public static void Spinner(Vector2 center, float radius)
    {
        if (dot == null) dot = Rounded(Color.white, Color.white, 16, 0, 34);
        var tint = GUI.color;
        int head = (int)(Time.realtimeSinceStartup * 12f) % 12;
        for (int i = 0; i < 12; i++)
        {
            float angle = i * Mathf.PI / 6f;
            float age = (head - i + 12) % 12 / 12f;
            GUI.color = new Color(Gold.r, Gold.g, Gold.b, Mathf.Lerp(1f, 0.12f, age));
            float size = radius * 0.28f;
            var p = center + new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)) * radius;
            GUI.DrawTexture(new Rect(p.x - size / 2f, p.y - size / 2f, size, size), dot);
        }
        GUI.color = tint;
    }

    /// <summary>Named styles: title, body, small, error, primary, secondary, row, rowSelected, field.</summary>
    public static GUIStyle Style(string name)
    {
        if (!cache.TryGetValue(name, out var style))
        {
            style = Create(name);
            cache[name] = style;
        }
        return style;
    }

    static GUIStyle Create(string name)
    {
        switch (name)
        {
            case "brand": return Label(46, Text, FontStyle.Bold);
            case "brandSub": return Label(24, Gold, FontStyle.Bold);
            case "title": return Label(52, Text, FontStyle.Bold, TextAnchor.MiddleCenter);
            case "titleLeft": return Label(52, Text, FontStyle.Bold);
            case "body": return Label(32, Muted, FontStyle.Normal, TextAnchor.MiddleCenter, true);
            case "bodyLeft": return Label(32, Muted, FontStyle.Normal, TextAnchor.UpperLeft, true);
            case "label": return Label(30, Text, FontStyle.Bold);
            case "small": return Label(26, Muted, FontStyle.Normal, TextAnchor.UpperLeft, true);
            case "smallCenter": return Label(26, Muted, FontStyle.Normal, TextAnchor.MiddleCenter, true);
            case "error": return Label(30, ErrorColor, FontStyle.Normal, TextAnchor.MiddleCenter, true);
            case "errorLeft": return Label(26, ErrorColor, FontStyle.Normal, TextAnchor.UpperLeft, true);
            case "panel": return Box(Rounded(PanelFill, Edge, 28, 2), 28);
            case "track": return Box(Rounded(new Color32(0x08, 0x0D, 0x18, 0xFF), Edge, 14, 2), 14);
            case "fill": return Box(Rounded(Gold, Gold, 14, 0), 14);
            case "primary":
                return Button(38, Dark, Rounded(Gold, Gold, 22, 0),
                    Rounded(Color.Lerp(Gold, Color.black, 0.18f), Color.Lerp(Gold, Color.black, 0.18f), 22, 0), TextAnchor.MiddleCenter);
            case "secondary":
                return Button(36, Text, Rounded(RowFill, Edge, 22, 2), Rounded(RowPressed, Edge, 22, 2), TextAnchor.MiddleCenter);
            case "row":
                return Button(32, Text, Rounded(RowFill, Edge, 20, 2), Rounded(RowPressed, Edge, 20, 2), TextAnchor.MiddleLeft, 40);
            case "rowSelected":
                return Button(32, Text, Rounded(RowPressed, Gold, 20, 3), Rounded(RowPressed, Gold, 20, 3), TextAnchor.MiddleLeft, 40);
            case "field":
            {
                var bg = Rounded(FieldFill, Edge, 16, 2);
                var focused = Rounded(FieldFill, Gold, 16, 2);
                var s = new GUIStyle(GUI.skin.textField)
                {
                    fontSize = 34, alignment = TextAnchor.MiddleLeft, border = new RectOffset(16, 16, 16, 16),
                    padding = new RectOffset(24, 24, 0, 0), clipping = TextClipping.Clip,
                };
                s.normal.background = s.hover.background = s.active.background = bg;
                s.focused.background = s.onFocused.background = focused;
                s.normal.textColor = s.hover.textColor = s.active.textColor = s.focused.textColor = s.onFocused.textColor = Text;
                return s;
            }
            default: return new GUIStyle(GUI.skin.label);
        }
    }

    static GUIStyle Label(int size, Color color, FontStyle font, TextAnchor anchor = TextAnchor.MiddleLeft, bool wrap = false)
    {
        var s = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = font, alignment = anchor, wordWrap = wrap, richText = false };
        s.normal.textColor = color;
        return s;
    }

    static GUIStyle Box(Texture2D background, int radius)
    {
        var s = new GUIStyle { border = new RectOffset(radius, radius, radius, radius) };
        s.normal.background = background;
        return s;
    }

    static GUIStyle Button(int size, Color text, Texture2D normal, Texture2D pressed, TextAnchor anchor, int padding = 24)
    {
        var s = new GUIStyle
        {
            fontSize = size, fontStyle = FontStyle.Bold, alignment = anchor, border = new RectOffset(22, 22, 22, 22),
            padding = new RectOffset(padding, padding, 8, 8), clipping = TextClipping.Clip, wordWrap = false,
        };
        s.normal.background = s.hover.background = s.focused.background = normal;
        s.active.background = s.onActive.background = pressed;
        s.normal.textColor = s.hover.textColor = s.focused.textColor = s.active.textColor = s.onActive.textColor = text;
        return s;
    }

    /// <summary>Anti-aliased rounded rectangle for 9-slicing: fill, an edge of <paramref name="edge"/> px.</summary>
    static Texture2D Rounded(Color fill, Color edgeColor, int radius, int edge, int size = 0)
    {
        if (size == 0) size = radius * 2 + 4;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave };
        var pixels = new Color[size * size];
        float half = size / 2f, r = Mathf.Min(radius, half);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            // Signed distance to the rounded rectangle's outline (negative inside).
            float qx = Mathf.Abs(x + 0.5f - half) - (half - r), qy = Mathf.Abs(y + 0.5f - half) - (half - r);
            float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
            // Edge band of `edge` px inside the outline, blended over one pixel.
            var c = edge > 0 ? Color.Lerp(fill, edgeColor, Mathf.Clamp01(outside + edge + 0.5f)) : fill;
            c.a *= Mathf.Clamp01(0.5f - outside);
            pixels[y * size + x] = c;
        }
        tex.SetPixels(pixels);
        tex.Apply(false, true);
        return tex;
    }

    static Texture2D VerticalGradient(Color top, Color bottom)
    {
        var tex = new Texture2D(1, 256, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
        for (int y = 0; y < 256; y++) tex.SetPixel(0, y, Color.Lerp(bottom, top, y / 255f));
        tex.Apply(false, true);
        return tex;
    }

    static Texture2D RadialGlow()
    {
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f)) / (size / 2f);
            float a = Mathf.Clamp01(1f - d);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
        }
        tex.Apply(false, true);
        return tex;
    }
}
