using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(SepiaRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Sepia", true)]
public sealed class Sepia : PostProcessEffectSettings
{
	[Tooltip("Sepia color")]
	public ColorParameter Color = new ColorParameter
	{
		value = new Color(0.6745098f, 0.4784314f, 0.2f, 0f)
	};
}
