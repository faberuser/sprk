using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(DrunkRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Drunk", true)]
public sealed class Drunk : PostProcessEffectSettings
{
	[Tooltip("Horizontal wobble")]
	public FloatParameter Horizontal = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("Vertical wobble")]
	public FloatParameter Vertical = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("Animation speed")]
	public FloatParameter Speed = new FloatParameter
	{
		value = 0f
	};
}
