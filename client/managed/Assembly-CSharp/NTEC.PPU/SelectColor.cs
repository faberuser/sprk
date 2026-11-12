using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(SelectColorRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/SelectColor", true)]
public sealed class SelectColor : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Effect intensity")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("Selected color")]
	public ColorParameter Selected = new ColorParameter
	{
		value = new Color(1f, 0f, 0f, 0f)
	};
}
