using UnityEngine;

namespace NVespa.NUtil;

public class TexTools
{
	public static Texture2D ConvertToTexture2D(RenderTexture rt, bool oneBitAlpha)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = rt;
		Texture2D texture2D = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
		if (oneBitAlpha)
		{
			Color32[] pixels = texture2D.GetPixels32();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(pixels); i++)
			{
				Color32 color = pixels[i];
				if (color.a > 0)
				{
					color.a = byte.MaxValue;
				}
				pixels[i] = color;
			}
			texture2D.SetPixels32(pixels);
		}
		RenderTexture.active = active;
		return texture2D;
	}
}
