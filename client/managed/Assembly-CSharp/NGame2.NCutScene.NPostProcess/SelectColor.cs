using System;
using LitJson;
using NTEC.PPU;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NPostProcess;

public class SelectColor : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndIntensity;

	public Color SelectedColor;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginIntensity;

	private NTEC.PPU.SelectColor _selectColor;

	public bool IsEnd { get; private set; }

	public SelectColor()
	{
		EndIntensity = 0.7f;
		SelectedColor = Color.red;
	}

	public void Begin(Func<NTEC.PPU.SelectColor> creator)
	{
		if (!IsEnable || creator == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_selectColor = Vespa_System_Action_Extension_Method.fire(creator);
		_selectColor.enabled.Override(IsEnable);
		_selectColor.Selected.Override(SelectedColor);
		_beginIntensity = _selectColor.Intensity;
		UpdateSettings(_beginIntensity);
	}

	private void UpdateSettings(float i)
	{
		_selectColor.Intensity.Override(i);
		_selectColor.active = i > 0f;
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable && !(_selectColor == null))
		{
			UpdateSettings(EndIntensity);
		}
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd && !(_selectColor == null))
		{
			float i;
			if (BeginTick == EndTick)
			{
				i = EndIntensity;
			}
			else
			{
				float num = NVespa.Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num);
				i = Mathf.Lerp(_beginIntensity, EndIntensity, t);
			}
			UpdateSettings(i);
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
		SelectedColor = json["SelectedColor"].GETCOLOR(SelectedColor);
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
		jsonData.AddFieldColor("SelectedColor", SelectedColor);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
