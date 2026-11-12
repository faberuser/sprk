using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

[Serializable]
[PostProcess(typeof(EightColors2Renderer), PostProcessEvent.AfterStack, "NTEC/Screen/EightColors2", true)]
public sealed class EightColors2 : PostProcessEffectSettings
{
	[Tooltip("Base color")]
	public ColorParameter Start = new ColorParameter
	{
		value = new Color(0f, 0f, 0f, 0f)
	};

	[Tooltip("0.000 to 0.125 luminosity")]
	public ColorParameter OneEighth = new ColorParameter
	{
		value = new Color(0.1215686f, 0.1215686f, 0.1215686f, 0f)
	};

	[Tooltip("0.125 to 0.250 luminosity")]
	public ColorParameter TwoEights = new ColorParameter
	{
		value = new Color(0.2470588f, 0.2470588f, 0.2470588f, 0f)
	};

	[Tooltip("0.250 to 0.375 luminosity")]
	public ColorParameter ThreeEights = new ColorParameter
	{
		value = new Color(0.372549f, 0.372549f, 0.372549f, 0f)
	};

	[Tooltip("0.375 to 0.500 luminosity")]
	public ColorParameter FourEights = new ColorParameter
	{
		value = new Color(0.4980392f, 0.4980392f, 0.4980392f, 0f)
	};

	[Tooltip("0.500 to 0.675 luminosity")]
	public ColorParameter FiveEights = new ColorParameter
	{
		value = new Color(0.6235294f, 0.6235294f, 0.6235294f, 0f)
	};

	[Tooltip("0.675 to 0.750 luminosity")]
	public ColorParameter SixEights = new ColorParameter
	{
		value = new Color(0.7490196f, 0.7490196f, 0.7490196f, 0f)
	};

	[Tooltip("0.750 to 0.875 luminosity")]
	public ColorParameter SevenEights = new ColorParameter
	{
		value = new Color(0.8745098f, 0.8745098f, 0.8745098f, 0f)
	};

	[Tooltip("0.875 to 1.000 luminosity")]
	public ColorParameter Full = new ColorParameter
	{
		value = new Color(1f, 1f, 1f, 0f)
	};
}
