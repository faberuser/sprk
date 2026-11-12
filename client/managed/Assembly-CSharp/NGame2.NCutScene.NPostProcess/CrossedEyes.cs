using System;
using LitJson;
using NTEC.PPU;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class CrossedEyes : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndShift;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginShift;

	private NTEC.PPU.CrossedEyes _crossedEyes;

	public bool IsEnd { get; private set; }

	public CrossedEyes()
	{
		EndShift = 0.5f;
	}

	public void Begin(Func<NTEC.PPU.CrossedEyes> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_crossedEyes = Vespa_System_Action_Extension_Method.fire(creator);
		_crossedEyes.enabled.Override(IsEnable);
		_beginShift = _crossedEyes.Shift;
		UpdateSettings(_beginShift);
	}

	private void UpdateSettings(float shift)
	{
		_crossedEyes.Shift.Override(shift);
		_crossedEyes.active = shift > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_crossedEyes == null))
		{
			UpdateSettings(EndShift);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_crossedEyes == null))
		{
			float shift;
			if (BeginTick == EndTick)
			{
				shift = EndShift;
			}
			else
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				shift = Mathf.Lerp(_beginShift, EndShift, t);
			}
			UpdateSettings(shift);
			if (elapsedTick >= EndTick)
			{
				IsEnd = true;
			}
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		EndShift = json["EndShift"].GETFLOAT(EndShift);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		Curve = JsonTools.ToCurve(json["Curve"], Curve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldDouble("EndShift", EndShift);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
