using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(ChromaticAberrationRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/ChromaticAberration", true)]
public sealed class ChromaticAberration : PostProcessEffectSettings
{
	[Range(1f, 8f)]
	[Tooltip("Effect iterations")]
	public IntParameter Iterations = new IntParameter
	{
		value = 1
	};

	[Range(0f, 1f)]
	[Tooltip("Channels offset")]
	public FloatParameter Offset = new FloatParameter
	{
		value = 0.5f
	};

	internal RenderTexture loopRT0;

	internal RenderTexture loopRT1;
}
