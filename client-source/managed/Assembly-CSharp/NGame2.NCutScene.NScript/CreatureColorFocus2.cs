using System.Collections.Generic;
using LitJson;
using NGame2.NCutScene.NModifier;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NCutScene.NScript;

public class CreatureColorFocus2 : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public class SlotColor : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public int SlotIndex;

		public Color FocusColorMix;

		public SlotColor()
		{
			SlotIndex = 0;
			FocusColorMix = Colors.zero;
		}

		bool ILoadJson.Load(JsonData json)
		{
			SlotIndex = json["SlotIndex"].GETINT(SlotIndex);
			FocusColorMix = json["FocusColorMix"].GETCOLOR(FocusColorMix);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldLong("SlotIndex", SlotIndex);
			jsonData.AddFieldColor("FocusColorMix", FocusColorMix);
			return jsonData;
		}
	}

	public bool AllFocus;

	public Color AllFocusColorMix = Colors.zero;

	public int FocusCount = 1;

	public List<SlotColor> FocusSlotList = new List<SlotColor>();

	public float FocusTimeLength;

	public Color OtherColorMix = new Color(0f, 0f, 0f, 0.9f);

	public bool EndReleaseFocus;

	public float ReleaseTimeLength;

	bool IScript.IsEnd => true;

	public CreatureColorFocus2()
	{
		AllFocusColorMix = Colors.zero;
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
			CreatureModifier creatureModByIndex = SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr.GetCreatureModByIndex(i);
			Color color = OtherColorMix;
			if (AllFocus)
			{
				color = AllFocusColorMix;
			}
			else
			{
				bool result = false;
				Color color2 = TryGetFocusSlotColor(slotIndexByIndex, out result);
				if (result)
				{
					color = color2;
				}
			}
			creatureModByIndex.SetColorMix(color, FocusTimeLength);
		}
	}

	private Color TryGetFocusSlotColor(int slot, out bool result)
	{
		SlotColor slotColor = FocusSlotList.Find((SlotColor element) => element.SlotIndex == slot);
		result = slotColor != null;
		if (!result)
		{
			return OtherColorMix;
		}
		return slotColor.FocusColorMix;
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
		AllFocusColorMix = json["AllFocusColorMix"].GETCOLOR(AllFocusColorMix);
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
		jsonData.AddFieldColor("AllFocusColorMix", AllFocusColorMix);
		jsonData.AddFieldColor("OtherColorMix", OtherColorMix);
		jsonData.AddFieldDouble("FocusTimeLength", FocusTimeLength);
		jsonData.AddFieldDouble("ReleaseTimeLength", ReleaseTimeLength);
		jsonData.AddFieldBool("EndReleaseFocus", EndReleaseFocus);
		return jsonData;
	}
}
