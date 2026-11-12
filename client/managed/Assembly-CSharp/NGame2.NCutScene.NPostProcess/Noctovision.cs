using System;
using LitJson;
using NTEC.PPU;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class Noctovision : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndGreen;

	public float EndNoise;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginGreen;

	private float _beginNoise;

	private NTEC.PPU.Noctovision _noctoVision;

	public bool IsEnd { get; private set; }

	public Noctovision()
	{
		EndGreen = 0.75f;
		EndNoise = 0.5f;
	}

	public void Begin(Func<NTEC.PPU.Noctovision> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_noctoVision = Vespa_System_Action_Extension_Method.fire(creator);
		_noctoVision.enabled.Override(IsEnable);
		_beginGreen = _noctoVision.Green;
		_beginNoise = _noctoVision.Noise;
		UpdateSettings(_beginGreen, _beginNoise);
	}

	private void UpdateSettings(float green, float noise)
	{
		_noctoVision.Green.Override(green);
		_noctoVision.Noise.Override(noise);
		_noctoVision.active = green > 0f || noise > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_noctoVision == null))
		{
			UpdateSettings(EndGreen, EndNoise);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_noctoVision == null))
		{
			float green;
			float noise;
			if (BeginTick == EndTick)
			{
				green = EndGreen;
				noise = EndNoise;
			}
			else
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				green = Mathf.Lerp(_beginGreen, EndGreen, t);
				noise = Mathf.Lerp(_beginNoise, EndNoise, t);
			}
			UpdateSettings(green, noise);
			if (elapsedTick >= EndTick)
			{
				IsEnd = true;
			}
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		EndGreen = json["EndGreen"].GETFLOAT(EndGreen);
		EndNoise = json["EndNoise"].GETFLOAT(EndNoise);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		Curve = JsonTools.ToCurve(json["Curve"], Curve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldDouble("EndGreen", EndGreen);
		jsonData.AddFieldDouble("EndNoise", EndNoise);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
