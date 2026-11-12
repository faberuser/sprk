using System.Collections.Generic;
using LitJson;
using NGame2.NUI.NManager;
using UnityEngine;

namespace NGame2.NGameSetting;

public class EntryMenuData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public class ShopAppearanceData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		bool ILoadJson.Load(JsonData json)
		{
			if (json.isNone())
			{
				return false;
			}
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			return new JsonData();
		}
	}

	private List<NGame2.NUI.NManager.EntryMenuListData> menuList = new List<NGame2.NUI.NManager.EntryMenuListData>();

	public float BlendTimeLength = 0.4f;

	public float PopupTimeLength = 0.2f;

	public AnimationCurve PopupCurve = new AnimationCurve();

	public AnimationCurve NpcSlideCurve = new AnimationCurve();

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		BlendTimeLength = json["BlendTimeLength"].GETFLOAT(BlendTimeLength);
		PopupTimeLength = json["PopupTimeLength"].GETFLOAT(PopupTimeLength);
		PopupCurve = json["PopupCurve"].GetAnimationCurve(PopupCurve);
		NpcSlideCurve = json["NpcSlideCurve"].GetAnimationCurve(NpcSlideCurve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("BlendTimeLength", BlendTimeLength);
		jsonData.AddFieldDouble("PopupTimeLength", PopupTimeLength);
		jsonData.AddFieldAnimationCurve("PopupCurve", PopupCurve);
		jsonData.AddFieldAnimationCurve("NpcSlideCurve", NpcSlideCurve);
		return jsonData;
	}
}
