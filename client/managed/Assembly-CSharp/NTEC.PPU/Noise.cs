using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(NoiseRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Noise", true)]
public sealed class Noise : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Effect intenisty")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0f
	};

	[Range(0f, 1f)]
	[Tooltip("Noise saturation")]
	public FloatParameter Saturation = new FloatParameter
	{
		value = 0f
	};

	internal RenderTexture tempRT0;
}
