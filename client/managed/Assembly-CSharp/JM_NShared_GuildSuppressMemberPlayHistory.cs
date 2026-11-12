using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressMemberPlayHistory
{
	public static GuildSuppressMemberPlayHistory Parse(string s)
	{
		return Parse(s, new GuildSuppressMemberPlayHistory());
	}

	public static GuildSuppressMemberPlayHistory Parse(string s, GuildSuppressMemberPlayHistory o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressMemberPlayHistory Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressMemberPlayHistory());
	}

	public static GuildSuppressMemberPlayHistory Parse(IDictionary d, GuildSuppressMemberPlayHistory o)
	{
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.NickName = BaseJsonMarshaler.ParseValueString(d, "NickName");
		o.BattleTime = BaseJsonMarshaler.ParseValueString(d, "BattleTime");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressMemberPlayHistory o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressMemberPlayHistory o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.NickName != null)
		{
			dictionary.Add("NickName", o.NickName);
		}
		if (o.BattleTime != null)
		{
			dictionary.Add("BattleTime", o.BattleTime);
		}
		dictionary.Add("Score", o.Score.ToString());
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("Level", o.Level.ToString());
		return dictionary;
	}
}
