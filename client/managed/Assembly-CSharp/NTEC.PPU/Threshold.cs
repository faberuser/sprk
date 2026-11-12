using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(ThresholdRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Threshold", true)]
public sealed class Threshold : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Effect visibility")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0f
	};

	[Range(0f, 1f)]
	[Tooltip("Threshold level")]
	public FloatParameter Level = new FloatParameter
	{
		value = 0f
	};
}
