using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(OldFilmRenderer), PostProcessEvent.AfterStack, "NTEC/Screen/OldFilm", true)]
public sealed class OldFilm : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Noise intensity")]
	public FloatParameter Noise = new FloatParameter
	{
		value = 0f
	};

	[Range(0f, 1f)]
	[Tooltip("Oscillation amplitude")]
	public FloatParameter Oscillation = new FloatParameter
	{
		value = 0f
	};

	internal RenderTexture tempRT0;

	internal RenderTexture tempRT1;

	internal RenderTexture tempRT2;
}
