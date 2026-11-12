using System;
using LitJson;
using NTEC.PPU;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class IndustrialCamera : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndIntensity;

	public float EndLines;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginIntensity;

	private float _beginLines;

	private NTEC.PPU.IndustrialCamera _industrialCamera;

	public bool IsEnd { get; private set; }

	public IndustrialCamera()
	{
		EndIntensity = 0.5f;
		EndLines = 0.5f;
	}

	public void Begin(Func<NTEC.PPU.IndustrialCamera> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_industrialCamera = Vespa_System_Action_Extension_Method.fire(creator);
		_industrialCamera.enabled.Override(IsEnable);
		_beginLines = _industrialCamera.Lines;
		_beginIntensity = _industrialCamera.Intensity;
		UpdateSettings(_beginLines, _beginIntensity);
	}

	private void UpdateSettings(float lines, float intensity)
	{
		_industrialCamera.Lines.Override(lines);
		_industrialCamera.Intensity.Override(intensity);
		_industrialCamera.active = intensity > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_industrialCamera == null))
		{
			UpdateSettings(EndLines, EndIntensity);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_industrialCamera == null))
		{
			float intensity;
			float lines;
			if (BeginTick == EndTick)
			{
				intensity = EndIntensity;
				lines = EndLines;
			}
			else
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				intensity = Mathf.Lerp(_beginIntensity, EndIntensity, t);
				lines = Mathf.Lerp(_beginLines, EndLines, t);
			}
			UpdateSettings(lines, intensity);
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
		EndLines = json["EndLines"].GETFLOAT(EndLines);
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
		jsonData.AddFieldDouble("EndLines", EndLines);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
