using LitJson;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NCutScene;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NCutScene.NModifier;

public class TextWindowAlphaModifier : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable = true;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve AlphaCurve = new AnimationCurve();

	public bool IsEnd { get; private set; }

	private CutSceneWindow Window => SceneAutoBehaviour<CommonManagement>.instance.GetInstance<CutSceneWindow>();

	public void Begin()
	{
		if (!IsEnable)
		{
			IsEnd = true;
			return;
		}
		float windowAlpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(AlphaCurve, 0f, 0f);
		Window.SetWindowAlpha(windowAlpha);
		IsEnd = false;
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd)
		{
			if (BeginTick < EndTick)
			{
				float num = Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float windowAlpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(AlphaCurve, num, num);
				Window.SetWindowAlpha(windowAlpha);
			}
			if (elapsedTick >= EndTick)
			{
				IsEnd = true;
			}
		}
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable)
		{
			float windowAlpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(AlphaCurve, 1f, 1f);
			Window.SetWindowAlpha(windowAlpha);
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		AlphaCurve = json["AlphaCurve"].GetAnimationCurve(AlphaCurve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddFieldAnimationCurve("AlphaCurve", AlphaCurve);
		return jsonData;
	}
}
