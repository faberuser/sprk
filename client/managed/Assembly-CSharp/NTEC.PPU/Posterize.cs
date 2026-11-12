using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(PosterizeRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Posterize", true)]
public sealed class Posterize : PostProcessEffectSettings
{
	[Tooltip("Number of colors in each channel")]
	public IntParameter Colors = new IntParameter
	{
		value = 8
	};
}
