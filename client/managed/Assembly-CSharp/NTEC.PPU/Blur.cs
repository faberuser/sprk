using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(BlurRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Blur", true)]
public sealed class Blur : PostProcessEffectSettings
{
	[Range(1f, 16f)]
	[Tooltip("Effect Iterations")]
	public IntParameter Iterations = new IntParameter
	{
		value = 4
	};

	[Range(0f, 1f)]
	[Tooltip("Effect Intensity")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0.5f
	};

	internal RenderTexture loopRT0;

	internal RenderTexture loopRT1;
}
