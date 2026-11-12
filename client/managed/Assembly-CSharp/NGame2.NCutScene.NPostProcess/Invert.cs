using System;
using LitJson;
using NTEC.PPU;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class Invert : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndIntensity;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginIntensity;

	private NTEC.PPU.Invert _invert;

	public bool IsEnd { get; private set; }

	public Invert()
	{
		EndIntensity = 1f;
	}

	public void Begin(Func<NTEC.PPU.Invert> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_invert = Vespa_System_Action_Extension_Method.fire(creator);
		_invert.enabled.Override(IsEnable);
		_beginIntensity = _invert.Intensity;
		UpdateSettings(_beginIntensity);
	}

	private void UpdateSettings(float intensity)
	{
		_invert.Intensity.Override(intensity);
		_invert.active = intensity > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_invert == null))
		{
			UpdateSettings(EndIntensity);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_invert == null))
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
