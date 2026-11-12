using System;
using LitJson;
using NTEC.PPU;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class OldFilm : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndNoise;

	public float EndOscillation;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginNoise;

	private float _beginOscillation;

	private NTEC.PPU.OldFilm _oldFilm;

	public bool IsEnd { get; private set; }

	public OldFilm()
	{
		EndNoise = 0.5f;
		EndOscillation = 0.5f;
	}

	public void Begin(Func<NTEC.PPU.OldFilm> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_oldFilm = Vespa_System_Action_Extension_Method.fire(creator);
		_oldFilm.enabled.Override(IsEnable);
		_beginNoise = _oldFilm.Noise;
		_beginOscillation = _oldFilm.Oscillation;
		UpdateSettings(_beginNoise, _beginOscillation);
	}

	private void UpdateSettings(float noise, float oscillation)
	{
		_oldFilm.Noise.Override(noise);
		_oldFilm.Oscillation.Override(oscillation);
		_oldFilm.active = noise > 0f || oscillation > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_oldFilm == null))
		{
			UpdateSettings(EndNoise, EndOscillation);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_oldFilm == null))
		{
			float noise;
			float oscillation;
			if (BeginTick == EndTick)
			{
				noise = EndNoise;
				oscillation = EndOscillation;
			}
			else
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				noise = Mathf.Lerp(_beginNoise, EndNoise, t);
				oscillation = Mathf.Lerp(_beginOscillation, EndOscillation, t);
			}
			UpdateSettings(noise, oscillation);
			if (elapsedTick >= EndTick)
			{
				IsEnd = true;
			}
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		EndNoise = json["EndNoise"].GETFLOAT(EndNoise);
		EndOscillation = json["EndOscillation"].GETFLOAT(EndOscillation);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		Curve = JsonTools.ToCurve(json["Curve"], Curve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldDouble("EndNoise", EndNoise);
		jsonData.AddFieldDouble("EndOscillation", EndOscillation);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
