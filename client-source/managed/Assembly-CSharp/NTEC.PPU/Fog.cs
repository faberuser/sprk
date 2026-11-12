using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(FogRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Fog", true)]
public sealed class Fog : PostProcessEffectSettings
{
	[Tooltip("Fog color")]
	public ColorParameter Color = new ColorParameter
	{
		value = new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.7843137f)
	};

	[Range(0f, 1f)]
	[Tooltip("Fog visibility")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 1f)]
	[Tooltip("Distance at which fog should appear")]
	public FloatParameter Distance = new FloatParameter
	{
		value = 1f
	};

	[Tooltip("Fog height")]
	public FloatParameter Height = new FloatParameter
	{
		value = 3f
	};
}
