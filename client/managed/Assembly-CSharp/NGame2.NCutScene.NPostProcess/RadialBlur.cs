using System;
using LitJson;
using NTEC.PPU;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class RadialBlur : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndIntensity;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginIntensity;

	private RadialBlur2 _blur;

	public bool IsEnd { get; private set; }

	public RadialBlur()
	{
		EndIntensity = 0.3f;
	}

	public void Begin(Func<RadialBlur2> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_blur = Vespa_System_Action_Extension_Method.fire(creator);
		_blur.enabled.Override(IsEnable);
		_beginIntensity = _blur.Intensity;
		UpdateSettings(_beginIntensity);
	}

	private void UpdateSettings(float intensity)
	{
		_blur.Intensity.Override(intensity);
		_blur.active = intensity > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_blur == null))
		{
			UpdateSettings(EndIntensity);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_blur == null))
		{
			float intensity;
			if (BeginTick == EndTick)
			{
				intensity = EndIntensity;
			}
			else
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				intensity = Mathf.Lerp(_beginIntensity, EndIntensity, t);
			}
			UpdateSettings(intensity);
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
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
