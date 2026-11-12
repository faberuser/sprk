using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(ScanLinesRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/ScanLines", true)]
public sealed class ScanLines : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Effect intensity")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 1f)]
	[Tooltip("Line width")]
	public FloatParameter Width = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 1f)]
	[Tooltip("Gap between lines")]
	public FloatParameter Gap = new FloatParameter
	{
		value = 0.5f
	};

	internal RenderTexture tempRT0;
}
