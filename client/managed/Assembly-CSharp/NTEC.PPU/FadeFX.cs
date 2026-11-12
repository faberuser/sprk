using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public class FadeFX : MonoBehaviour
{
	public PostProcessProfile profile;

	public float FadeSeconds = 5f;

	internal float timePassed;

	internal List<PostProcessEffectSettings> settings;

	internal Fade fx;

	private void Start()
	{
		if (!(profile != null))
		{
			return;
		}
		settings = profile.settings;
		foreach (PostProcessEffectSettings setting in settings)
		{
			if (setting.ToString().Contains("NTEC.PPU.Fade"))
			{
				fx = (Fade)setting;
				break;
			}
		}
	}

	private void Update()
	{
		if (fx != null)
		{
			fx.Intensity.value = (timePassed += Time.deltaTime) / FadeSeconds;
		}
	}
}
