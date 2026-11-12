using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaRecordInfo
{
	public static GuildArenaRecordInfo Parse(string s)
	{
		return Parse(s, new GuildArenaRecordInfo());
	}

	public static GuildArenaRecordInfo Parse(string s, GuildArenaRecordInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaRecordInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaRecordInfo());
	}

	public static GuildArenaRecordInfo Parse(IDictionary d, GuildArenaRecordInfo o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.LeftServerGroup = BaseJsonMarshaler.ParseValueString(d, "LeftServerGroup");
		o.LeftGuildId = BaseJsonMarshaler.ParseValueLong(d, "LeftGuildId");
		o.LeftAccountId = BaseJsonMarshaler.ParseValueLong(d, "LeftAccountId");
		o.RightServerGroup = BaseJsonMarshaler.ParseValueString(d, "RightServerGroup");
		o.RightGuildId = BaseJsonMarshaler.ParseValueLong(d, "RightGuildId");
		o.RightAccountId = BaseJsonMarshaler.ParseValueLong(d, "RightAccountId");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.Win = BaseJsonMarshaler.ParseValueInt(d, "Win");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(GuildArenaRecordInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaRecordInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		if (o.LeftServerGroup != null)
		{
			dictionary.Add("LeftServerGroup", o.LeftServerGroup);
		}
		dictionary.Add("LeftGuildId", o.LeftGuildId.ToString());
		dictionary.Add("LeftAccountId", o.LeftAccountId.ToString());
		if (o.RightServerGroup != null)
		{
			dictionary.Add("RightServerGroup", o.RightServerGroup);
		}
		dictionary.Add("RightGuildId", o.RightGuildId.ToString());
		dictionary.Add("RightAccountId", o.RightAccountId.ToString());
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		dictionary.Add("Win", o.Win.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
