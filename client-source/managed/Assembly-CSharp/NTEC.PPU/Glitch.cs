using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(GlitchRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Glitch", true)]
public sealed class Glitch : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Animation speed")]
	public FloatParameter Speed = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 1f)]
	[Tooltip("Bug size")]
	public FloatParameter Size = new FloatParameter
	{
		value = 0.3f
	};
}
