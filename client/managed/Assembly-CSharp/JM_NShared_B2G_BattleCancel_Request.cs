using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_BattleCancel;

public static class JM_NShared_B2G_BattleCancel_Request
{
	public static NShared.B2G_BattleCancel.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_BattleCancel.Request());
	}

	public static NShared.B2G_BattleCancel.Request Parse(string s, NShared.B2G_BattleCancel.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_BattleCancel.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_BattleCancel.Request());
	}

	public static NShared.B2G_BattleCancel.Request Parse(IDictionary d, NShared.B2G_BattleCancel.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseValueInt(d, "DungeonDifficulty");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_BattleCancel.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_BattleCancel.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DungeonDifficulty", o.DungeonDifficulty.ToString());
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		return dictionary;
	}
}
