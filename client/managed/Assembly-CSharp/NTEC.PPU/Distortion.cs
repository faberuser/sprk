using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(DistortionRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Distortion", true)]
public sealed class Distortion : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Effect intensity")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0.5f
	};

	[Tooltip("New coords")]
	public TextureParameter Coords = new TextureParameter
	{
		value = null
	};
}
