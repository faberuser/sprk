using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(NoctovisionRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Noctovision", true)]
public sealed class Noctovision : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Green intensity")]
	public FloatParameter Green = new FloatParameter
	{
		value = 0f
	};

	[Range(0f, 1f)]
	[Tooltip("Noise intenisty")]
	public FloatParameter Noise = new FloatParameter
	{
		value = 0f
	};

	internal RenderTexture tempRT0;
}
