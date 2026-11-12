using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MatchInfo
{
	public static MatchInfo Parse(string s)
	{
		return Parse(s, new MatchInfo());
	}

	public static MatchInfo Parse(string s, MatchInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MatchInfo Parse(IDictionary d)
	{
		return Parse(d, new MatchInfo());
	}

	public static MatchInfo Parse(IDictionary d, MatchInfo o)
	{
		o.Uid = BaseJsonMarshaler.ParseValueLong(d, "Uid");
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.AccountInfos = BaseJsonMarshaler.ParseArray(d, "AccountInfos", JM_NShared_MatchedAccountInfo.Parse);
		o.BattleServerAddress = BaseJsonMarshaler.ParseValueString(d, "BattleServerAddress");
		o.BattleServerPort = BaseJsonMarshaler.ParseValueInt(d, "BattleServerPort");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseEnum<DungeonDifficultyType>(d, "DungeonDifficulty");
		o.OnlineGameSpeedRatio = BaseJsonMarshaler.ParseValueInt(d, "OnlineGameSpeedRatio");
		o.ExtraData = BaseJsonMarshaler.ParseObject(d, "ExtraData", JM_NShared_BattleInstanceExtraData.Parse);
		o.CanCheat = BaseJsonMarshaler.ParseValueBool(d, "CanCheat");
		return o;
	}

	public static string EncodeJsonString(MatchInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MatchInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Uid", o.Uid.ToString());
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("AccountInfos", BaseJsonMarshaler.EncodeArray(o.AccountInfos, JM_NShared_MatchedAccountInfo.EncodeJsonObject));
		if (o.BattleServerAddress != null)
		{
			dictionary.Add("BattleServerAddress", o.BattleServerAddress);
		}
		dictionary.Add("BattleServerPort", o.BattleServerPort.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DungeonDifficulty", o.DungeonDifficulty.ToString());
		dictionary.Add("OnlineGameSpeedRatio", o.OnlineGameSpeedRatio.ToString());
		dictionary.Add("ExtraData", JM_NShared_BattleInstanceExtraData.EncodeJsonObject(o.ExtraData));
		dictionary.Add("CanCheat", o.CanCheat.ToString());
		return dictionary;
	}
}
