using System;
using LitJson;
using NVespa;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.NCutScene.NPostProcess;

public class Vignette : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public Vector2 EndCenter;

	public float EndRoundness;

	public bool IsRounded;

	public float EndIntensity;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private Vector2 _beginCenter;

	private float _beginIntensity;

	private float _beginRoundness;

	private UnityEngine.Rendering.PostProcessing.Vignette _vignette;

	public bool IsEnd { get; private set; }

	public Vignette()
	{
		EndCenter = new Vector2(0.5f, 0.5f);
		EndRoundness = 0.5f;
		EndIntensity = 0.5f;
	}

	public void Begin(Func<UnityEngine.Rendering.PostProcessing.Vignette> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_vignette = Vespa_System_Action_Extension_Method.fire(creator);
		_vignette.enabled.Override(IsEnable);
		_vignette.rounded.Override(IsRounded);
		_beginCenter = _vignette.center;
		_vignette.center.Override(_beginCenter);
		_beginRoundness = _vignette.roundness;
		_vignette.roundness.Override(_beginRoundness);
		_beginIntensity = _vignette.intensity;
		UpdateSettings(_beginIntensity);
	}

	private void UpdateSettings(float intensity)
	{
		_vignette.intensity.Override(intensity);
		_vignette.active = intensity > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_vignette == null))
		{
			_vignette.center.Override(EndCenter);
			_vignette.roundness.Override(EndRoundness);
			_vignette.intensity.Override(EndIntensity);
			UpdateSettings(EndIntensity);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_vignette == null))
		{
			Vector2 x = EndCenter;
			float intensity = EndIntensity;
			float x2 = EndRoundness;
			if (BeginTick != EndTick)
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				x = Vector2.Lerp(_beginCenter, EndCenter, t);
				intensity = Mathf.Lerp(_beginIntensity, EndIntensity, t);
				x2 = Mathf.Lerp(_beginRoundness, EndRoundness, t);
			}
			_vignette.center.Override(x);
			_vignette.roundness.Override(x2);
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
		EndCenter = JsonTools.ToVector2(json["EndCenter"], EndCenter);
		EndIntensity = json["EndIntensity"].GETFLOAT(EndIntensity);
		EndRoundness = json["EndRoundness"].GETFLOAT(EndRoundness);
		IsRounded = json["IsRounded"].GETBOOLEAN(IsRounded);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		Curve = JsonTools.ToCurve(json["Curve"], Curve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddField("EndCenter", JsonTools.FromVector2(EndCenter));
		jsonData.AddFieldDouble("EndIntensity", EndIntensity);
		jsonData.AddFieldDouble("EndRoundness", EndRoundness);
		jsonData.AddFieldBool("IsRounded", IsRounded);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
