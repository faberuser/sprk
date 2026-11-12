using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(FrameRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Frame", true)]
public sealed class Frame : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Horizontal frame")]
	public FloatParameter Horizontal = new FloatParameter
	{
		value = 0f
	};

	[Range(0f, 1f)]
	[Tooltip("Vertical frame")]
	public FloatParameter Vertical = new FloatParameter
	{
		value = 0f
	};
}
