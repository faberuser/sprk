using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressPlayInfo
{
	public static GuildSuppressPlayInfo Parse(string s)
	{
		return Parse(s, new GuildSuppressPlayInfo());
	}

	public static GuildSuppressPlayInfo Parse(string s, GuildSuppressPlayInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressPlayInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressPlayInfo());
	}

	public static GuildSuppressPlayInfo Parse(IDictionary d, GuildSuppressPlayInfo o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.GuildId = BaseJsonMarshaler.ParseValueInt(d, "GuildId");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.MonsterHp0 = BaseJsonMarshaler.ParseValueLong(d, "MonsterHp0");
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		o.KilledByMe = BaseJsonMarshaler.ParseValueBool(d, "KilledByMe");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressPlayInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressPlayInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("MonsterHp0", o.MonsterHp0.ToString());
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		dictionary.Add("KilledByMe", o.KilledByMe.ToString());
		return dictionary;
	}
}
