using System;
using LitJson;
using NVespa;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.NCutScene.NPostProcess;

public class ChromaticAberration : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndIntensity;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginIntensity;

	private UnityEngine.Rendering.PostProcessing.ChromaticAberration _chromaticAberration;

	public bool IsEnd { get; private set; }

	public ChromaticAberration()
	{
		EndIntensity = 0.5f;
	}

	public void Begin(Func<UnityEngine.Rendering.PostProcessing.ChromaticAberration> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_chromaticAberration = Vespa_System_Action_Extension_Method.fire(creator);
		_chromaticAberration.enabled.Override(IsEnable);
		_chromaticAberration.fastMode.Override(IsEnable);
		_beginIntensity = _chromaticAberration.intensity;
		UpdateSettings(_beginIntensity);
	}

	private void UpdateSettings(float intensity)
	{
		_chromaticAberration.intensity.Override(intensity);
		_chromaticAberration.active = intensity > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_chromaticAberration == null))
		{
			UpdateSettings(EndIntensity);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_chromaticAberration == null))
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
