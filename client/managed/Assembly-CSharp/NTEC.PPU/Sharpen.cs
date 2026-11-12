using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(SharpenRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Sharpen", true)]
public sealed class Sharpen : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Intensity of sharpening")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0.5f
	};

	internal RenderTexture tempRT0;
}
