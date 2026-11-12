using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressInfo
{
	public static GuildSuppressInfo Parse(string s)
	{
		return Parse(s, new GuildSuppressInfo());
	}

	public static GuildSuppressInfo Parse(string s, GuildSuppressInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressInfo());
	}

	public static GuildSuppressInfo Parse(IDictionary d, GuildSuppressInfo o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.GuidId = BaseJsonMarshaler.ParseValueInt(d, "GuidId");
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		o.AppliedTime = BaseJsonMarshaler.ParseValueString(d, "AppliedTime");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.MonsterHp0 = BaseJsonMarshaler.ParseValueLong(d, "MonsterHp0");
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		dictionary.Add("GuidId", o.GuidId.ToString());
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		if (o.AppliedTime != null)
		{
			dictionary.Add("AppliedTime", o.AppliedTime);
		}
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("MonsterHp0", o.MonsterHp0.ToString());
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		return dictionary;
	}
}
