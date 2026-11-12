using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(OverlayRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Overlay", true)]
public sealed class Overlay : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Effect intensity")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0.5f
	};

	[Tooltip("Overlay texture")]
	public TextureParameter Image = new TextureParameter
	{
		value = null
	};
}
