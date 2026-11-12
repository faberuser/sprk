using System;
using LitJson;
using NTEC.PPU;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class Threshold : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndIntensity;

	public float EndLevel;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginIntensity;

	private float _beginLevel;

	private NTEC.PPU.Threshold _threshold;

	public bool IsEnd { get; private set; }

	public Threshold()
	{
		EndIntensity = 0.5f;
		EndLevel = 0.5f;
	}

	public void Begin(Func<NTEC.PPU.Threshold> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_threshold = Vespa_System_Action_Extension_Method.fire(creator);
		_threshold.enabled.Override(IsEnable);
		_beginIntensity = _threshold.Intensity;
		_beginLevel = _threshold.Level;
		UpdateSettings(_beginIntensity, _beginLevel);
	}

	private void UpdateSettings(float i, float l)
	{
		_threshold.Level.Override(l);
		_threshold.Intensity.Override(i);
		_threshold.active = i > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_threshold == null))
		{
			UpdateSettings(EndIntensity, EndLevel);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_threshold == null))
		{
			float i;
			float l;
			if (BeginTick == EndTick)
			{
				i = EndIntensity;
				l = EndLevel;
			}
			else
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				i = Mathf.Lerp(_beginIntensity, EndIntensity, t);
				l = Mathf.Lerp(_beginLevel, EndLevel, t);
			}
			UpdateSettings(i, l);
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
		EndLevel = json["EndLevel"].GETFLOAT(EndLevel);
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
		jsonData.AddFieldDouble("EndLevel", EndLevel);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
