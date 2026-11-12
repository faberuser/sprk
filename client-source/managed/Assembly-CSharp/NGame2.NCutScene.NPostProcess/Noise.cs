using System;
using LitJson;
using NTEC.PPU;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class Noise : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndIntensity;

	public float EndSaturation;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginIntensity;

	private float _beginSaturation;

	private NTEC.PPU.Noise _noise;

	public bool IsEnd { get; private set; }

	public Noise()
	{
		EndIntensity = 0.5f;
		EndSaturation = 0.5f;
	}

	public void Begin(Func<NTEC.PPU.Noise> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_noise = Vespa_System_Action_Extension_Method.fire(creator);
		_noise.enabled.Override(IsEnable);
		_beginSaturation = _noise.Saturation;
		_beginIntensity = _noise.Intensity;
		UpdateSettings(_beginSaturation, _beginIntensity);
	}

	private void UpdateSettings(float saturation, float intensity)
	{
		_noise.Saturation.Override(saturation);
		_noise.Intensity.Override(intensity);
		_noise.active = intensity > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_noise == null))
		{
			UpdateSettings(EndSaturation, EndIntensity);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_noise == null))
		{
			float saturation;
			float intensity;
			if (BeginTick == EndTick)
			{
				saturation = EndSaturation;
				intensity = EndIntensity;
			}
			else
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				saturation = Mathf.Lerp(_beginSaturation, EndSaturation, t);
				intensity = Mathf.Lerp(_beginIntensity, EndIntensity, t);
			}
			UpdateSettings(saturation, intensity);
			if (elapsedTick >= EndTick)
			{
				IsEnd = true;
			}
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		EndIntensity = json["EndIntensity"].GETFLOAT(EndIntensity);
		EndSaturation = json["EndSaturation"].GETFLOAT(EndSaturation);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		Curve = JsonTools.ToCurve(json["Curve"], Curve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldDouble("EndIntensity", EndIntensity);
		jsonData.AddFieldDouble("EndSaturation", EndSaturation);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
