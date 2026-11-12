using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public class EffectsPresentation : MonoBehaviour
{
	public PostProcessProfile profile;

	internal List<PostProcessEffectSettings> settings;

	internal float timePassed;

	internal Fade fx;

	private void Start()
	{
		if (profile != null)
		{
			settings = profile.settings;
		}
	}

	private void Update()
	{
		if (Time.time > 1f && Time.time < 33f)
		{
			settings[(int)Mathf.Floor(Time.time)].active = true;
			settings[(int)Mathf.Floor(Time.time) - 1].active = false;
			if (settings[(int)Mathf.Floor(Time.time)].ToString().Contains("NTEC.PPU.Fade"))
			{
				fx = (Fade)settings[(int)Mathf.Floor(Time.time)];
				fx.Intensity.value = (timePassed += Time.deltaTime);
			}
		}
	}
}
