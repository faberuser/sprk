using System.Collections.Generic;
using LitJson;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NCutScene.NScript;

public class CreatureColorFocus : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool AllFocus;

	public int FocusCount = 1;

	public List<int> FocusSlotList = new List<int>();

	public float FocusTimeLength;

	public Color FocusColorMix = Colors.zero;

	public Color OtherColorMix = new Color(0f, 0f, 0f, 0.9f);

	public bool EndReleaseFocus;

	public float ReleaseTimeLength;

	bool IScript.IsEnd => true;

	public CreatureColorFocus()
	{
		FocusColorMix = Colors.zero;
		OtherColorMix = GameSetting.data.CutScene.ColorFocus_OtherColorDefault;
	}

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		for (int i = 0; i < SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr.Count; i++)
		{
			int slotIndexByIndex = SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr.GetSlotIndexByIndex(i);
			SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr.GetCreatureModByIndex(i).SetColorMix((AllFocus || FocusSlotList.Contains(slotIndexByIndex)) ? FocusColorMix : OtherColorMix, FocusTimeLength);
		}
	}

	void IScript.Update()
	{
	}

	void IScript.End()
	{
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
		if (EndReleaseFocus)
		{
			for (int i = 0; i < SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr.Count; i++)
			{
				SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr.GetCreatureModByIndex(i).SetColorMix(Colors.zero, ReleaseTimeLength);
			}
		}
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		AllFocus = json["AllFocus"].GETBOOLEAN(AllFocus);
		FocusCount = json["FocusCount"].GETINT(FocusCount);
		FocusSlotList.LoadFromJson(json["FocusSlotList"]);
		FocusColorMix = json["FocusColorMix"].GETCOLOR(FocusColorMix);
		OtherColorMix = json["OtherColorMix"].GETCOLOR(OtherColorMix);
		FocusTimeLength = json["FocusTimeLength"].GETFLOAT(FocusTimeLength);
		ReleaseTimeLength = json["ReleaseTimeLength"].GETFLOAT(ReleaseTimeLength);
		EndReleaseFocus = json["EndReleaseFocus"].GETBOOLEAN(EndReleaseFocus);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("AllFocus", AllFocus);
		jsonData.AddFieldLong("FocusCount", FocusCount);
		jsonData.AddField("FocusSlotList", Json_Extension_Method_IList.MakeJson(FocusSlotList));
		jsonData.AddFieldColor("FocusColorMix", FocusColorMix);
		jsonData.AddFieldColor("OtherColorMix", OtherColorMix);
		jsonData.AddFieldDouble("FocusTimeLength", FocusTimeLength);
		jsonData.AddFieldDouble("ReleaseTimeLength", ReleaseTimeLength);
		jsonData.AddFieldBool("EndReleaseFocus", EndReleaseFocus);
		return jsonData;
	}
}
