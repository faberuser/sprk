using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(Glitch2Renderer), PostProcessEvent.AfterStack, "NTEC/Screen/Glitch2", true)]
public sealed class Glitch2 : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Effect intensity")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0.3f
	};

	[Tooltip("Animation speed")]
	public FloatParameter Speed = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 1f)]
	[Tooltip("Glitch saturation")]
	public FloatParameter Saturation = new FloatParameter
	{
		value = 0.5f
	};

	[Tooltip("Number of lines")]
	public IntParameter Lines = new IntParameter
	{
		value = 128
	};
}
