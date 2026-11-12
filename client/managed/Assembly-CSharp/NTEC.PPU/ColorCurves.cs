using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(ColorCurvesRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/ColorCurves", true)]
public sealed class ColorCurves : PostProcessEffectSettings
{
	[SerializeField]
	internal Texture2D RedEVALUATION;

	[Tooltip("Red channel curve")]
	public SplineParameter Red = new SplineParameter
	{
		value = new Spline(new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f)), 0f, loop: false, new Vector2(0f, 1f))
	};

	[SerializeField]
	internal Texture2D GreenEVALUATION;

	[Tooltip("Green channel curve")]
	public SplineParameter Green = new SplineParameter
	{
		value = new Spline(new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f)), 0f, loop: false, new Vector2(0f, 1f))
	};

	[SerializeField]
	internal Texture2D BlueEVALUATION;

	[Tooltip("Blue channel curve")]
	public SplineParameter Blue = new SplineParameter
	{
		value = new Spline(new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f)), 0f, loop: false, new Vector2(0f, 1f))
	};

	[Tooltip("Image filter")]
	public ColorParameter Tint = new ColorParameter
	{
		value = new Color(1f, 1f, 1f, 0f)
	};

	public void CreateEvaluations()
	{
		Color black = Color.black;
		RedEVALUATION = new Texture2D(128, 1)
		{
			wrapMode = TextureWrapMode.Clamp
		};
		for (int i = 0; i < 128; i++)
		{
			black.a = Red.value.cachedData[i];
			RedEVALUATION.SetPixel(i, 0, black);
		}
		RedEVALUATION.Apply();
		GreenEVALUATION = new Texture2D(128, 1)
		{
			wrapMode = TextureWrapMode.Clamp
		};
		for (int j = 0; j < 128; j++)
		{
			black.a = Green.value.cachedData[j];
			GreenEVALUATION.SetPixel(j, 0, black);
		}
		GreenEVALUATION.Apply();
		BlueEVALUATION = new Texture2D(128, 1)
		{
			wrapMode = TextureWrapMode.Clamp
		};
		for (int k = 0; k < 128; k++)
		{
			black.a = Blue.value.cachedData[k];
			BlueEVALUATION.SetPixel(k, 0, black);
		}
		BlueEVALUATION.Apply();
	}
}
