using UnityEngine;

namespace NVespa;

public class Colors
{
	public static Color zero = new Color(0f, 0f, 0f, 0f);

	public static Color orange = new Color(1f, 0.5f, 0f);

	public static Color skyblue = new Color(0.2f, 0.67f, 1f);

	public static Color whiteGray = new Color(0.75f, 0.75f, 0.75f);

	public static Color wineRed = new Color(0.6f, 0.1f, 2f);

	private static Color[] LoopColor = new Color[9]
	{
		Color.red,
		(Color.red + orange) / 2f,
		orange,
		(orange + Color.yellow) / 2f,
		Color.yellow,
		(Color.yellow + Color.green) / 2f,
		Color.green,
		(Color.green + skyblue) / 2f,
		skyblue
	};

	public static Color GetLoopColor(int i_index)
	{
		return LoopColor[i_index % LoopColor.Length];
	}

	public static Color GetLoopColor(float frame)
	{
		int num = Math.ClampOver((int)(frame * (float)LoopColor.Length), 0, LoopColor.Length);
		return LoopColor[num];
	}
}
