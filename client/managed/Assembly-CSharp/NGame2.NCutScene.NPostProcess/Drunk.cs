using System;
using LitJson;
using NTEC.PPU;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class Drunk : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndHorizontal;

	public float EndVertical;

	public float EndSpeed;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginHorizontal;

	private float _beginVertical;

	private float _beginSpeed;

	private NTEC.PPU.Drunk _drunk;

	public bool IsEnd { get; private set; }

	public Drunk()
	{
		EndHorizontal = 1f;
		EndVertical = 1f;
		EndSpeed = 1f;
	}

	public void Begin(Func<NTEC.PPU.Drunk> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_drunk = Vespa_System_Action_Extension_Method.fire(creator);
		_drunk.enabled.Override(IsEnable);
		_beginHorizontal = _drunk.Horizontal;
		_beginVertical = _drunk.Vertical;
		_beginSpeed = _drunk.Speed;
		UpdateSettings(_beginHorizontal, _beginVertical, _beginSpeed);
	}

	private void UpdateSettings(float hor, float ver, float spd)
	{
		_drunk.Horizontal.Override(hor);
		_drunk.Vertical.Override(ver);
		_drunk.Speed.Override(spd);
		_drunk.active = hor != 0f || ver != 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_drunk == null))
		{
			UpdateSettings(EndHorizontal, EndVertical, EndSpeed);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_drunk == null))
		{
			float hor;
			float ver;
			float spd;
			if (BeginTick == EndTick)
			{
				hor = EndHorizontal;
				ver = EndVertical;
				spd = EndSpeed;
			}
			else
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				hor = Mathf.Lerp(_beginHorizontal, EndHorizontal, t);
				ver = Mathf.Lerp(_beginVertical, EndVertical, t);
				spd = Mathf.Lerp(_beginSpeed, EndSpeed, t);
			}
			UpdateSettings(hor, ver, spd);
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
		EndSpeed = json["EndSpeed"].GETFLOAT(EndSpeed);
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
		jsonData.AddFieldDouble("EndSpeed", EndSpeed);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
