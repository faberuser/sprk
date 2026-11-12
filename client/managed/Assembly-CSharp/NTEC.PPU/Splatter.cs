using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(SplatterRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Splatter", true)]
public sealed class Splatter : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Effect intensity")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("Situation texture")]
	public TextureParameter Image = new TextureParameter
	{
		value = null
	};
}
