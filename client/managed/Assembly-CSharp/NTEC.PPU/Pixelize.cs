using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(PixelizeRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/Pixelize", true)]
public sealed class Pixelize : PostProcessEffectSettings
{
	[Tooltip("Horizontal resolution")]
	public IntParameter Horizontal = new IntParameter
	{
		value = 0
	};

	[Tooltip("Vertical resolution")]
	public IntParameter Vertical = new IntParameter
	{
		value = 0
	};
}
