using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(EdgeDetectionRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/EdgeDetection", true)]
public sealed class EdgeDetection : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Edge Offset")]
	public FloatParameter Edge = new FloatParameter
	{
		value = 0.5f
	};
}
