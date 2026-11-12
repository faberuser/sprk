using UnityEngine;

namespace NVespa.NGui;

public class DrawTools
{
	private static Texture2D _whiteTex;

	private static Texture2D _blackTex;

	public static Texture2D WhiteTex
	{
		get
		{
			if (_whiteTex == null)
			{
				_whiteTex = CreateBlankTex(Color.white);
			}
			return _whiteTex;
		}
	}

	public static Texture2D BlackTex
	{
		get
		{
			if (_blackTex == null)
			{
				_blackTex = CreateBlankTex(Color.black);
			}
			return _blackTex;
		}
	}

	private static Texture2D CreateBlankTex(Color color)
	{
		Texture2D texture2D = new Texture2D(4, 4);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				texture2D.SetPixel(j, i, color);
			}
		}
		texture2D.Apply(updateMipmaps: true);
		return texture2D;
	}

	public static void DrawSeparator(Color i_color)
	{
		DrawSeparator(i_color, 3f, i_horizonBlock: false);
	}

	public static void DrawSeparatorNoSpace(Color i_color, float i_thickness, bool i_horizonBlock)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			lastRect = ((!i_horizonBlock) ? Vespa_Unity_Rect_Extension_Method.EnlargeRect(lastRect, -2f, 0f) : Vespa_Unity_Rect_Extension_Method.EnlargeRect(lastRect, 0f, -2f));
			RepaintDrawSeparator(lastRect, i_color, i_thickness, 0f, i_horizonBlock);
		}
		GUILayout.Space(i_thickness);
	}

	public static void DrawSeparator(Color i_color, float i_thickness, bool i_horizonBlock)
	{
		if (Event.current.type == EventType.Repaint)
		{
			RepaintDrawSeparator(GUILayoutUtility.GetLastRect(), i_color, i_thickness, 4f, i_horizonBlock);
		}
		GUILayout.Space(5f + i_thickness);
	}

	public static Rect RepaintDrawSeparator(Rect i_rect, Color i_color, float i_thickness, float i_space, bool i_horizonBlock)
	{
		if (Event.current.type != EventType.Repaint)
		{
			return default(Rect);
		}
		Texture2D whiteTex = WhiteTex;
		Color color = GUI.color;
		GUI.color = i_color;
		if (i_horizonBlock)
		{
			GUI.DrawTexture(new Rect(i_rect.xMax + i_space, i_rect.yMin, i_thickness, i_rect.height), whiteTex);
			GUI.DrawTexture(new Rect(i_rect.xMax + i_space, i_rect.yMin, 1f, i_rect.height), whiteTex);
			GUI.DrawTexture(new Rect(i_rect.xMax + i_space - 1f + i_thickness, i_rect.yMin, 1f, i_rect.height), whiteTex);
			GUI.color = color;
			return new Rect(i_rect.xMax + i_space, i_rect.yMin, i_thickness, i_rect.height);
		}
		GUI.DrawTexture(new Rect(i_rect.xMin, i_rect.yMax + i_space, i_rect.width, i_thickness), whiteTex);
		GUI.DrawTexture(new Rect(i_rect.xMin, i_rect.yMax + i_space, i_rect.width, 1f), whiteTex);
		GUI.DrawTexture(new Rect(i_rect.xMin, i_rect.yMax + i_space - 1f + i_thickness, i_rect.width, 1f), whiteTex);
		GUI.color = color;
		return new Rect(i_rect.xMin, i_rect.yMax + i_space, i_rect.width, i_thickness);
	}

	public static void DrawGauge(Color i_bgColor, Color i_gaugeColor, float i_value)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Texture2D whiteTex = WhiteTex;
			Rect lastRect = GUILayoutUtility.GetLastRect();
			Color color = GUI.color;
			GUI.color = i_bgColor;
			GUI.DrawTexture(new Rect(lastRect.xMin, lastRect.yMax + 4f, lastRect.width, 5f), whiteTex);
			GUI.color = i_gaugeColor;
			GUI.DrawTexture(new Rect(lastRect.xMin, lastRect.yMax + 5f, lastRect.width * Mathf.Clamp01(i_value), 3f), whiteTex);
			GUI.color = color;
		}
		GUILayout.Space(10f);
	}

	public static void DrawOutline(Rect rect, Color color)
	{
		DrawOutline(rect, color, 1f);
	}

	public static void LastRectOutline(Color color)
	{
		DrawOutline(GUILayoutUtility.GetLastRect(), color);
	}

	public static void LastRectOutline(Color color, float i_thickness)
	{
		DrawOutline(GUILayoutUtility.GetLastRect(), color, i_thickness);
	}

	public static void DrawOutline(Rect rect, Color color, float i_thickness)
	{
		if (Event.current.type == EventType.Repaint)
		{
			float num = i_thickness * 0.5f;
			Color color2 = GUI.color;
			Texture2D whiteTex = WhiteTex;
			GUI.color = color;
			GUI.DrawTexture(Rect.MinMaxRect(rect.xMin - num, rect.yMin - num, rect.xMin + num, rect.yMax + num), whiteTex);
			GUI.DrawTexture(Rect.MinMaxRect(rect.xMax - num, rect.yMin - num, rect.xMax + num, rect.yMax + num), whiteTex);
			GUI.DrawTexture(Rect.MinMaxRect(rect.xMin - num, rect.yMin - num, rect.xMax + num, rect.yMin + num), whiteTex);
			GUI.DrawTexture(Rect.MinMaxRect(rect.xMin - num, rect.yMax - num, rect.xMax + num, rect.yMax + num), whiteTex);
			GUI.color = color2;
		}
	}

	public static void DrawBox(Rect rect, Color color)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Color color2 = GUI.color;
			Texture2D whiteTex = WhiteTex;
			GUI.color = color;
			GUI.DrawTexture(rect, whiteTex);
			GUI.color = color2;
		}
	}

	public static Rect DrawOutlineLayout(Color i_color, int i_width, int i_height)
	{
		GUILayout.Label("", GUILayout.Width(i_width), GUILayout.Height(i_height));
		Rect lastRect = GUILayoutUtility.GetLastRect();
		DrawOutline(lastRect, i_color);
		return lastRect;
	}

	public static Rect DrawTextureLayout(Texture2D i_tex)
	{
		return DrawTextureLayout(i_tex, i_tex.width, i_tex.height);
	}

	public static Rect DrawTextureLayout(Texture i_tex, int i_width, int i_height)
	{
		GUILayout.Label("", GUILayout.Width(i_width), GUILayout.Height(i_height));
		Rect lastRect = GUILayoutUtility.GetLastRect();
		if (Event.current.type == EventType.Repaint)
		{
			GUI.DrawTexture(lastRect, i_tex);
		}
		return lastRect;
	}

	public static Rect DrawTextureLayout(Texture i_tex, Color i_bgColor, int i_width, int i_height)
	{
		GUILayout.Label("", GUILayout.Width(i_width), GUILayout.Height(i_height));
		Rect lastRect = GUILayoutUtility.GetLastRect();
		if (Event.current.type == EventType.Repaint)
		{
			DrawBox(lastRect, i_bgColor);
			GUI.DrawTexture(lastRect, i_tex);
		}
		return lastRect;
	}
}
