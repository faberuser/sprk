using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(HorizontalBloomRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/HorizontalBloom", true)]
public sealed class HorizontalBloom : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Effect intensity")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 1f)]
	[Tooltip("Minimum luminosity")]
	public FloatParameter Threshold = new FloatParameter
	{
		value = 0.9f
	};

	[Range(1f, 3f)]
	[Tooltip("Number of blur iterations")]
	public IntParameter Quality = new IntParameter
	{
		value = 1
	};

	internal RenderTexture tempRT0;

	internal RenderTexture tempRT1;

	internal RenderTexture loopRT0;

	internal RenderTexture loopRT1;
}
