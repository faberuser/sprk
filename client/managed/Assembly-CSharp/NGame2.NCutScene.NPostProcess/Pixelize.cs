using System;
using LitJson;
using NTEC.PPU;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class Pixelize : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public int EndHorizontal;

	public int EndVertical;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private int _beginHorizontal;

	private int _beginVertical;

	private NTEC.PPU.Pixelize _pixelize;

	public bool IsEnd { get; private set; }

	public Pixelize()
	{
		EndHorizontal = 16;
		EndVertical = 16;
	}

	public void Begin(Func<NTEC.PPU.Pixelize> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_pixelize = Vespa_System_Action_Extension_Method.fire(creator);
		_pixelize.enabled.Override(IsEnable);
		_beginHorizontal = _pixelize.Horizontal;
		_beginVertical = _pixelize.Vertical;
		UpdateSettings(_beginHorizontal, _beginVertical);
	}

	private void UpdateSettings(int h, int v)
	{
		_pixelize.Horizontal.Override(h);
		_pixelize.Vertical.Override(v);
		_pixelize.active = h > 0 && v > 0;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_pixelize == null))
		{
			UpdateSettings(EndHorizontal, EndVertical);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_pixelize == null))
		{
			int h;
			int v;
			if (BeginTick == EndTick)
			{
				h = EndHorizontal;
				v = EndVertical;
			}
			else
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				h = (int)Mathf.Lerp(_beginHorizontal, EndHorizontal, t);
				v = (int)Mathf.Lerp(_beginVertical, EndVertical, t);
			}
			UpdateSettings(h, v);
			if (elapsedTick >= EndTick)
			{
				IsEnd = true;
			}
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		EndHorizontal = json["EndHorizontal"].GETINT(EndHorizontal);
		EndVertical = json["EndVertical"].GETINT(EndVertical);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		Curve = JsonTools.ToCurve(json["Curve"], Curve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldLong("EndHorizontal", EndHorizontal);
		jsonData.AddFieldLong("EndVertical", EndVertical);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
