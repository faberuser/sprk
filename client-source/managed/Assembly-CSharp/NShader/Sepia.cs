using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NShader;

[Serializable]
[PostProcess(typeof(SepiaRenderer), PostProcessEvent.AfterStack, "Custom/Sepia", true)]
public sealed class Sepia : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Sepia effect intensity.")]
	public FloatParameter blend = new FloatParameter
	{
		value = 0f
	};
}
