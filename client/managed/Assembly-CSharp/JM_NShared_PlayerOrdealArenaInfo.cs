using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerOrdealArenaInfo
{
	public static PlayerOrdealArenaInfo Parse(string s)
	{
		return Parse(s, new PlayerOrdealArenaInfo());
	}

	public static PlayerOrdealArenaInfo Parse(string s, PlayerOrdealArenaInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerOrdealArenaInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerOrdealArenaInfo());
	}

	public static PlayerOrdealArenaInfo Parse(IDictionary d, PlayerOrdealArenaInfo o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.SelectedTierType = BaseJsonMarshaler.ParseValueString(d, "SelectedTierType");
		o.RefreshCount = BaseJsonMarshaler.ParseValueInt(d, "RefreshCount");
		o.DeadHeroInfo = BaseJsonMarshaler.ParseValueString(d, "DeadHeroInfo");
		o.BattleNodeIndices = BaseJsonMarshaler.ParseValueString(d, "BattleNodeIndices");
		o.EventNodeIndices = BaseJsonMarshaler.ParseValueString(d, "EventNodeIndices");
		o.ClearNodeIndices = BaseJsonMarshaler.ParseValueString(d, "ClearNodeIndices");
		o.SelectedBuffIndices = BaseJsonMarshaler.ParseValueString(d, "SelectedBuffIndices");
		o.SelectBuffIndices = BaseJsonMarshaler.ParseValueString(d, "SelectBuffIndices");
		o.Score = BaseJsonMarshaler.ParseValueInt(d, "Score");
		return o;
	}

	public static string EncodeJsonString(PlayerOrdealArenaInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerOrdealArenaInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		if (o.SelectedTierType != null)
		{
			dictionary.Add("SelectedTierType", o.SelectedTierType);
		}
		dictionary.Add("RefreshCount", o.RefreshCount.ToString());
		if (o.DeadHeroInfo != null)
		{
			dictionary.Add("DeadHeroInfo", o.DeadHeroInfo);
		}
		if (o.BattleNodeIndices != null)
		{
			dictionary.Add("BattleNodeIndices", o.BattleNodeIndices);
		}
		if (o.EventNodeIndices != null)
		{
			dictionary.Add("EventNodeIndices", o.EventNodeIndices);
		}
		if (o.ClearNodeIndices != null)
		{
			dictionary.Add("ClearNodeIndices", o.ClearNodeIndices);
		}
		if (o.SelectedBuffIndices != null)
		{
			dictionary.Add("SelectedBuffIndices", o.SelectedBuffIndices);
		}
		if (o.SelectBuffIndices != null)
		{
			dictionary.Add("SelectBuffIndices", o.SelectBuffIndices);
		}
		dictionary.Add("Score", o.Score.ToString());
		return dictionary;
	}
}
