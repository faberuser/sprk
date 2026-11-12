using System;
using LitJson;
using NTEC.PPU;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class Frame : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndHorizontal;

	public float EndVertical;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginHorizontal;

	private float _beginVertical;

	private NTEC.PPU.Frame _frame;

	public bool IsEnd { get; private set; }

	public Frame()
	{
		EndHorizontal = 0.5f;
		EndVertical = 0.5f;
	}

	public void Begin(Func<NTEC.PPU.Frame> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_frame = Vespa_System_Action_Extension_Method.fire(creator);
		_frame.enabled.Override(IsEnable);
		_beginHorizontal = _frame.Horizontal.value;
		_beginVertical = _frame.Vertical.value;
		UpdateSettings(_beginHorizontal, _beginVertical);
	}

	private void UpdateSettings(float hor, float ver)
	{
		_frame.Horizontal.Override(hor);
		_frame.Vertical.Override(ver);
		_frame.active = hor > 0f || ver > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_frame == null))
		{
			UpdateSettings(EndHorizontal, EndVertical);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_frame == null))
		{
			float hor;
			float ver;
			if (BeginTick == EndTick)
			{
				hor = EndHorizontal;
				ver = EndVertical;
			}
			else
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				hor = Mathf.Lerp(_beginHorizontal, EndHorizontal, t);
				ver = Mathf.Lerp(_beginVertical, EndVertical, t);
			}
			UpdateSettings(hor, ver);
			if (elapsedTick >= EndTick)
			{
				IsEnd = true;
			}
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		EndHorizontal = json["EndHorizontal"].GETFLOAT(EndHorizontal);
		EndVertical = json["EndVertical"].GETFLOAT(EndVertical);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		Curve = JsonTools.ToCurve(json["Curve"], Curve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldDouble("EndHorizontal", EndHorizontal);
		jsonData.AddFieldDouble("EndVertical", EndVertical);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
