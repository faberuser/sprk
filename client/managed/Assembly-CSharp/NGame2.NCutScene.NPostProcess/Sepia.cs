using System;
using LitJson;
using NShader;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class Sepia : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndBlend;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginBlend;

	private NShader.Sepia _sepia;

	public bool IsEnd { get; private set; }

	public Sepia()
	{
		EndBlend = 0.5f;
	}

	public void Begin(Func<NShader.Sepia> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_sepia = Vespa_System_Action_Extension_Method.fire(creator);
		_sepia.enabled.Override(IsEnable);
		_beginBlend = _sepia.blend;
		UpdateSettings(_beginBlend);
	}

	private void UpdateSettings(float blend)
	{
		_sepia.blend.Override(blend);
		_sepia.active = blend > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_sepia == null))
		{
			UpdateSettings(EndBlend);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_sepia == null))
		{
			float blend;
			if (BeginTick == EndTick)
			{
				blend = EndBlend;
			}
			else
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				blend = Mathf.Lerp(_beginBlend, EndBlend, t);
			}
			UpdateSettings(blend);
			if (elapsedTick >= EndTick)
			{
				IsEnd = true;
			}
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		EndBlend = json["EndBlend"].GETFLOAT(EndBlend);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		Curve = JsonTools.ToCurve(json["Curve"], Curve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldDouble("EndBlend", EndBlend);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
