using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DispatchBattleInfo
{
	public static DispatchBattleInfo Parse(string s)
	{
		return Parse(s, new DispatchBattleInfo());
	}

	public static DispatchBattleInfo Parse(string s, DispatchBattleInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DispatchBattleInfo Parse(IDictionary d)
	{
		return Parse(d, new DispatchBattleInfo());
	}

	public static DispatchBattleInfo Parse(IDictionary d, DispatchBattleInfo o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Difficulty = BaseJsonMarshaler.ParseValueInt(d, "Difficulty");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.RepeatCount = BaseJsonMarshaler.ParseValueInt(d, "RepeatCount");
		o.HeroIndices = BaseJsonMarshaler.ParseValueString(d, "HeroIndices");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.CompleteTime = BaseJsonMarshaler.ParseValueString(d, "CompleteTime");
		o.ClientResult = BaseJsonMarshaler.ParseValueString(d, "ClientResult");
		o.ClientResultTimeMs = BaseJsonMarshaler.ParseValueInt(d, "ClientResultTimeMs");
		o.ServerResult = BaseJsonMarshaler.ParseValueString(d, "ServerResult");
		o.State = BaseJsonMarshaler.ParseEnum<DispatchState>(d, "State");
		o.WinCount = BaseJsonMarshaler.ParseValueInt(d, "WinCount");
		o.LoseCount = BaseJsonMarshaler.ParseValueInt(d, "LoseCount");
		o.StaminaDiscountRate = BaseJsonMarshaler.ParseValueInt(d, "StaminaDiscountRate");
		return o;
	}

	public static string EncodeJsonString(DispatchBattleInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DispatchBattleInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		dictionary.Add("RepeatCount", o.RepeatCount.ToString());
		if (o.HeroIndices != null)
		{
			dictionary.Add("HeroIndices", o.HeroIndices);
		}
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.CompleteTime != null)
		{
			dictionary.Add("CompleteTime", o.CompleteTime);
		}
		if (o.ClientResult != null)
		{
			dictionary.Add("ClientResult", o.ClientResult);
		}
		dictionary.Add("ClientResultTimeMs", o.ClientResultTimeMs.ToString());
		if (o.ServerResult != null)
		{
			dictionary.Add("ServerResult", o.ServerResult);
		}
		dictionary.Add("State", o.State.ToString());
		dictionary.Add("WinCount", o.WinCount.ToString());
		dictionary.Add("LoseCount", o.LoseCount.ToString());
		dictionary.Add("StaminaDiscountRate", o.StaminaDiscountRate.ToString());
		return dictionary;
	}
}
