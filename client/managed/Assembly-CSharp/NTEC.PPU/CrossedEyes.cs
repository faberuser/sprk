using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(CrossedEyesRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/CrossedEyes", true)]
public sealed class CrossedEyes : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Horizontal shift")]
	public FloatParameter Shift = new FloatParameter
	{
		value = 0f
	};
}
