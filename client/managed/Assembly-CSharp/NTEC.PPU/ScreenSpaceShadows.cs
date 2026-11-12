using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(ScreenSpaceShadowsRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/ScreenSpaceShadows", true)]
public sealed class ScreenSpaceShadows : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Effect intenisty")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0f
	};

	[Range(2f, 16f)]
	[Tooltip("Effect power")]
	public IntParameter Power = new IntParameter
	{
		value = 2
	};

	internal RenderTexture tempRT0;

	internal RenderTexture tempRT1;
}
