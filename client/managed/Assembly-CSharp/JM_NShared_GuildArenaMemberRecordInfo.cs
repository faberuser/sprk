using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaMemberRecordInfo
{
	public static GuildArenaMemberRecordInfo Parse(string s)
	{
		return Parse(s, new GuildArenaMemberRecordInfo());
	}

	public static GuildArenaMemberRecordInfo Parse(string s, GuildArenaMemberRecordInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaMemberRecordInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaMemberRecordInfo());
	}

	public static GuildArenaMemberRecordInfo Parse(IDictionary d, GuildArenaMemberRecordInfo o)
	{
		o.LeftTeam = BaseJsonMarshaler.ParseObject(d, "LeftTeam", JM_NShared_GuildMemberServerInfo.Parse);
		o.RightTeam = BaseJsonMarshaler.ParseObject(d, "RightTeam", JM_NShared_GuildMemberServerInfo.Parse);
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.Win = BaseJsonMarshaler.ParseValueInt(d, "Win");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(GuildArenaMemberRecordInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaMemberRecordInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("LeftTeam", JM_NShared_GuildMemberServerInfo.EncodeJsonObject(o.LeftTeam));
		dictionary.Add("RightTeam", JM_NShared_GuildMemberServerInfo.EncodeJsonObject(o.RightTeam));
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		dictionary.Add("Win", o.Win.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
