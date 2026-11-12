using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(IndustrialCameraRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/IndustrialCamera", true)]
public sealed class IndustrialCamera : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Effect intensity")]
	public FloatParameter Intensity = new FloatParameter
	{
		value = 0f
	};

	[Range(0f, 1f)]
	[Tooltip("Lines visibility")]
	public FloatParameter Lines = new FloatParameter
	{
		value = 0f
	};

	internal RenderTexture tempRT0;
}
