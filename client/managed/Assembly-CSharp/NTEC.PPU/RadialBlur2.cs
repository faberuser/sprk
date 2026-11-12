using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(RadialBlur2Renderer), PostProcessEvent.AfterStack, "NTEC/Screen/RadialBlur2", true)]
public sealed class RadialBlur2 : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Effect intensity")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0f
	};
}
